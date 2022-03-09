﻿using AConfig;
using NeoCortexApi.Entities;
using NeoCortexApi;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using Daenet.ImageBinarizerLib;
using Daenet.ImageBinarizerLib.Entities;

namespace ConsoleApp
{
    internal class Experiment
    {
        HtmConfig htmConfig;
        ArgsConfig expConfig;
        public Experiment(ArgsConfig config)
        {
            expConfig = config;
            htmConfig = config.htmConfig;
        }

        public void run()
        {
            int height = htmConfig.InputDimensions[0];
            int width = htmConfig.InputDimensions[1];
            Dictionary<string, int[]> sdrs;

            // By default it only returns subdirectories one level deep. 
            var directories = Directory.GetDirectories(expConfig.inputFolder).ToList();

            (Dictionary<string, int[]> binaries, // List of Binarized images
                Dictionary<string, List<string>> inputsPath // Path of the list of images found in the given folder
            ) = imageBinarization(directories, width, height);

            // The key of the dictionary helps to keep track of which class the SDR belongs to
            (sdrs, var cortexLayer) = SPTrain(htmConfig, binaries);

            HelpersTemp helperFunc = new HelpersTemp();

            Dictionary<string, double> listCorrelation = new();
            Dictionary<string, double> ListInputcorelation = new();

            foreach (KeyValuePair<string, List<string>> entry in inputsPath) // loop of the folder (classes) eg: Apple, banana, etc
            {
                var classLabel = entry.Key;
                var filePathList = entry.Value;
                var numberOfImages = filePathList.Count;

                for (int i = 0; i < numberOfImages; i++) // loop of the images inside the folder
                {
                    if (!sdrs.TryGetValue(filePathList[i], out int[] sdr1)) continue;

                    foreach (KeyValuePair<string, List<string>> secondEntry in inputsPath)
                    { // loop of the folder (again)
                        var classLabel2 = secondEntry.Key;
                        var filePathList2 = secondEntry.Value;
                        var numberOfImages2 = filePathList2.Count;
                        for (int j = 0; j < numberOfImages2; j++) // loop of the images inside the folder
                        {
                            if (!sdrs.TryGetValue(filePathList2[j], out int[] sdr2)) continue;
                            string fileNameofFirstImage = Path.GetFileNameWithoutExtension(filePathList[i]);
                            string fileNameOfSecondImage = Path.GetFileNameWithoutExtension(filePathList2[j]);
                            string temp = $"{classLabel + fileNameofFirstImage}__{classLabel2 + fileNameOfSecondImage}";
                            listCorrelation.Add(temp, MathHelpers.CalcArraySimilarity(sdr1, sdr2));
                            //find the similarity between the pictures from same folder
                            ListInputcorelation.Add(temp, MathHelpers.CalcArraySimilarity(binaries[filePathList[i]].IndexWhere((el) => el == 1), binaries[filePathList2[j]].IndexWhere((el) => el == 1)));
                        }
                    }
                }
            }

            var classes = inputsPath.Keys.ToList();
            //helperFunc.printSimilarityMatrix(listCorrelation, "micro", classes);
            //helperFunc.printSimilarityMatrix(listCorrelation, "macro", classes);
            //helperFunc.printSimilarityMatrix(listCorrelation, "both", classes);
            //Console.WriteLine(ListInputcorelation["Cabbagepic1__CabbagePic2"]);
            //input file encoding
            // passing the SDR values and given image SDR value after image binarization to the funstion PredictLabel           
            int[] encodedInputImage = ReadImageData("C:/SE/neocortexapi-classification/ImageClassification/ImageClassification/InputFolder/Cabbage/r0_0.jpg", width, height);
            var temp1 = cortexLayer.Compute(encodedInputImage, true);
            var activeColumns = cortexLayer.GetResult("sp") as int[];
            var sdrOfInputImage = activeColumns.OrderBy(c => c).ToArray();
            string predictedLabel = PredictLabel(sdrOfInputImage, sdrs);
            //Console.WriteLine($"Selected image path to predict label is  { inputsPath}");
            Console.WriteLine($"The label predicted is  { predictedLabel}");
            Console.ReadLine();
        }

        private Tuple<Dictionary<string, int[]>, Dictionary<string, List<string>>> imageBinarization(List<string> directories, int width, int height)
        {
            Dictionary<string, List<string>> inputsPath = new Dictionary<string, List<string>>();
            Dictionary<string, int[]> binaries = new Dictionary<string, int[]>();

            foreach (var fullPath in directories)
            {
                string folderName = Path.GetFileName(fullPath);

                if (!inputsPath.ContainsKey(folderName))
                {
                    inputsPath[folderName] = new List<string>();
                }

                var filePathList = Directory.GetFiles(fullPath).Where(name => !name.EndsWith(".txt")).ToList();


                foreach (var filePath in filePathList)
                {

                    inputsPath[folderName].Add(filePath);

                    // Image binarization
                    int[] inputVector = ReadImageData(filePath, height, width);
                    binaries.Add(filePath, inputVector);

                    // Write binarized data to a file
                    var baseDir = Path.GetDirectoryName(filePath);
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                    var ext = "txt";

                    var fullFileName = $"{fileNameWithoutExt}.{ext}";

                    System.IO.File.WriteAllLines(Path.Combine(baseDir, fullFileName), inputVector.Select(tb => tb.ToString()));
                }
            }
            return Tuple.Create(binaries, inputsPath);
        }

        /// <summary>
        /// Returns Binarized Image in integer array
        /// </summary>
        /// <param name="imagePath">Name of Image to be binarized</param>
        /// <param name="height">Height of Binarized Image</param>
        /// <param name="width">Width of Binarized Image</param>
        /// Parameter modified by Tiniya Vinod to check the result accuracy
        /// <returns></returns>

        public int[] ReadImageData(string imagePath, int height, int width)
        {
            var parameters = new BinarizerParams
            {
                InputImagePath = imagePath,
                ImageHeight = height,
                ImageWidth = width,
                BlueThreshold = 201,
                RedThreshold = 202,
                GreenThreshold = 205
            };
            ImageBinarizer bizer = new ImageBinarizer(parameters);

            var doubleArray = bizer.GetArrayBinary();
            var hg = doubleArray.GetLength(1);
            var wd = doubleArray.GetLength(0);
            var intArray = new int[hg * wd];
            for (int j = 0; j < hg; j++)
            {
                for (int i = 0; i < wd; i++)
                {
                    intArray[j * wd + i] = (int)doubleArray[i, j, 0];
                }
            }
            return intArray;
        }
        /// <summary> Modified by Long Nguyen
        ///           Pulling out SDRs after HPC fires a STABLE event when training the SP with list of patterns
        /// </summary>
        /// <param name="cfg"></param> Spatial Pooler configuration by HtmConfig style
        /// <param name="inputValues"></param> Binary input vector (pattern) list
        private static (Dictionary<string, int[]>, CortexLayer<object, object> cortexLayer) SPTrain(HtmConfig cfg, Dictionary<string, int[]> inputValues)
        {
            // Creates the htm memory.
            var mem = new Connections(cfg);
            bool isInStableState = false;
            // HPC extends the default Spatial Pooler algorithm.
            // The purpose of HPC is to set the SP in the new-born stage at the begining of the learning process.
            // In this stage the boosting is very active, but the SP behaves instable. After this stage is over
            // (defined by the second argument) the HPC is controlling the learning process of the SP.
            // Once the SDR generated for every input gets stable, the HPC will fire event that notifies your code
            // that SP is stable now.
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
                (isStable, numPatterns, actColAvg, seenInputs) =>
                {
                    // Event should only be fired when entering the stable state.
                    // Ideal SP should never enter unstable state after stable state.
                    if (isStable == false)
                    {
                        Console.WriteLine($"INSTABLE STATE");
                        // This should usually not happen.
                        isInStableState = false;
                    }
                    else
                    {
                        Console.WriteLine($"STABLE STATE");
                        // Here you can perform any action if required.
                        isInStableState = true;
                    }
                });

            // It creates the instance of Spatial Pooler Multithreaded version.
            SpatialPooler sp = new SpatialPoolerMT(hpa);

            // Initializes the Spatial Pooler 
            sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });

            // mem.TraceProximalDendritePotential(true);

            // It creates the instance of the neo-cortex layer.
            // Algorithm will be performed inside of that layer.
            CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");

            // Add encoder as the very first module. This model is connected to the sensory input cells
            // that receive the input. Encoder will receive the input and forward the encoded signal
            // to the next module.
            //cortexLayer.HtmModules.Add("encoder", encoder);

            // The next module in the layer is Spatial Pooler. This module will receive the output of the
            // encoder.
            cortexLayer.HtmModules.Add("sp", sp);

            // Learning process will take 1000 iterations (cycles)
            int maxSPLearningCycles = 1;

            // Save the result SDR into a list of array
            Dictionary<string, int[]> outputValues = new Dictionary<string, int[]>();

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                Console.WriteLine($"Cycle  ** {cycle} ** Stability: {isInStableState}");

                int iteration = 0;
                outputValues.Clear(); // Remove all elements in output SDR list

                // This trains the layer on input pattern.

                foreach (var input in inputValues)
                {
                    iteration++;

                    // Learn the input pattern.
                    // Output lyrOut is the output of the last module in the layer.

                    var lyrOut = cortexLayer.Compute(input.Value, true);

                    // This is a general way to get the SpatialPooler result from the layer.
                    var activeColumns = cortexLayer.GetResult("sp") as int[];

                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    outputValues[input.Key] = actCols;
                }
                if (isInStableState)
                    break;
            }
            return (outputValues, cortexLayer);
        }
        /// <summary>
        /// To find out the label prediction of the given image
        /// Created by Tiniya on 08.02.2022
        /// </summary>
        /// <param name="sdrOfInputImage"></param>
        /// <param name="sdrs"></param>
        /// <returns></returns>
        public string PredictLabel(int[] sdrOfInputImage, Dictionary<string, int[]> sdrs)
        {
            string label = "Couldnot able to predict the label";
            foreach (var k1 in sdrs)
            {
                Boolean isArrayEqual = true;
                int[] newarray = k1.Value;
                isArrayEqual = sdrOfInputImage.SequenceEqual(newarray);
                if (isArrayEqual)
                {
                    label = k1.Key.ToString();
                    string[] labelarray = label.Split('\\');
                    label = labelarray[10];
                    return label;
                }
            }
            return label;
        }
    }
}
