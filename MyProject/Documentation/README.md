# Analyse Image Classification (Fruits 360 dataset) 
Team members: Anu Maria Varghese, Tiniya Vinod Puthanpurayil, Veena Alphonsa Jose

# **Project Description**
## **AIM**
In the previous work at the university an Image Classification solution has been implemented. Our task is to implement a program that uses the existing solution as a library and start a training of learning images. The image dataset we have used in this project is Fruits 360 dataset images. Our task is to change various learning parameters and to find the best fit that shows image classification. Most important learning parameters are: Global/Local Inhibition, Potential Radius, Local Area Density and NumofActiveColumnsPerInArea. We have to demonstrate how these parameters influence the learning. Our code must provide the learning code and prediction code. After learning user should use our code and test the quality of learning. For example, the user after learning enter the image “table”. The prediction code provide a set of predicting results like: “Table – 87%, chair 7%, light - 3%”.

## METHODS
### 1 Prepare the program's directory:
First we have prepared images that are required for the training. Images must be copied in the following folder structure along with the application and the config json:  

<img width="342" alt="WorkingDirectory" src="https://user-images.githubusercontent.com/93146556/158149017-3d3b8973-bc09-4490-ae05-297dd9256cb4.png">
 
The imagesets are stored inside "InputFolder/".  

 <img width="476" alt="InputFolder" src="https://user-images.githubusercontent.com/93146556/158149083-9d965452-8a43-40e2-81f9-fbb77ddaaec9.png">


Each Imageset is stored inside a folder whose name is the set's label.  

<img width="396" alt="SampleData" src="https://user-images.githubusercontent.com/93146556/158149133-b93f8bc3-cfe4-49af-872c-ffd97f20d964.png">


 Sample input folder of the project can be found [here](https://github.com/VeenaAlphonsa/neocortexapi-classification/tree/main/ImageClassification/ImageClassification/InputFolder)  
 ### 2 Start the application by passing required command line arguments
 ~~~csharp
 ImageClassifier -if "InputFolder" -cf htmconfig.json
 ~~~
 -if   "Input Images folder path"  
 -cf   "json htm config file path"  
 
 **HTM Configuration**  
 HTM setting of the project can be inputted to the program by means of a .json file [htmconfig.json](https://github.com/VeenaAlphonsa/neocortexapi-classification/blob/main/ImageClassification/ImageClassification/htmconfig.json).  
 Multiple experiments can therefore be conducted via changes of parameters in the json file. 
 For a reference on what each parameter does, please refer to []() on [neocortexapi](https://github.com/ddobric/neocortexapi) 
 
### 3 How it works

When started the application will load images and start the training process. The training process runs in following steps.

#### (1) Convert The Images to binary array via binarization**  
[The Binarization Library](https://github.com/daenetCorporation/imagebinarizer) was developed as an open source project at [Daenet](https://daenet.de/de/).  
the current implementation uses a color threshold of 200 for every color in a 8bit-RGB scale.  
The images with the same label must be stored in folder. The folder name is the images' label.   

#### (2) Learn spatial patterns stored in images with the Spatial Pooler(SP)
SP first iterates through all images until the stable state is entered.
SP iterate through all the images as it learns.

#### (3) Validation of SP Learning for different set of images
The last set of Sparse Density Representations (SDRs), the output of Spatial Pooler(SP) for each binarized image were saved for correlation validation.  
There are 2 types of correlation which are defined as follow:
1. *Micro Correlation*: Maximum/Average/Minimum correlation in similar bit percent of all images' SDRs which respect to each another in the same label.  
2. *Macro Correlation*: Maximum/Average/Minimum correlation in similar bit percent of all images' SDRs with images from 2 different labels.   
The results of the two correlation are printed in the command prompt when executing the code  

The algorithm for calculating correlation can be found [here](https://github.com/ddobric/neocortexapi/blob/7d05b61b919a82fd7f8028c63970dfbc7d78dd50/source/NeoCortexApi/Utility/MathHelpers.cs#L93)  
Result example:

![Result18-Output Example](https://user-images.githubusercontent.com/93146556/158153392-b655405b-9491-4273-b479-d82e8d776ca0.jpg)

The Images used was collected from [Fruit 360](https://www.kaggle.com/moltean/fruits).  

#### (4) How to run the application in Visual Studio
Visual Studio can add arguments (args) parameter to run your code.  
![](Images/LaunchProfile.png)
This is done by changing the arguments command line arguments in Debug Properties Launch Profiles to 
~~~csharp
-cf htmconfig.json -if "InputFolder"
~~~
-cf add the option of the configuration file "htmconfig.json"  
-if add the option of the training Input Folder "InputFolder/".  
This folder contains folders of images, where the folder names also act as the label for the images inside it.  

## APPROACH
#### 1. By changing various HTM Parameter to find the best fit correlation Matrix
Our task is to change various learning parameters and to find the best fit that shows image classification. Most important learning parameters are: Global/Local Inhibition, Potential Radius, Local Area Density and NumofActiveColumnsPerInArea and we found how these parameters influenced learning. After conducting various tests we have been able to find the parameters at which we get the least overlapping inbetween Micro and Macro and thus the best correlation matrix.



#### 2. To Predict the Input Label
We have compared the SDRs of the input label with the SDRs of the existing dataset and predicted the input label. The prediction code will give the name of the label which is being predicted with the highest similiarity. Below is the prediction code.

~~~csharp
public string PredictLabel(int[] sdrOfInputImage, Dictionary<string, int[]> sdrs)
        {
            string label = "Could not able to predict the label";
            foreach (var k1 in sdrs)
            {
                Boolean isArrayEqual = true;
                int[] newarray = k1.Value;
                isArrayEqual = sdrOfInputImage.SequenceEqual(newarray);
                if (isArrayEqual)
                {
                    label = k1.Key.ToString();
                    string[] labelarray = label.Split('\\');
                    label = labelarray[11];
                    return label;
                }
            }
            return label;
        }
~~~



#### 3. To modify the prediction code to calculate the highest similiarity of the input images
To test the quality of learning we have improved the prediction code to calculate the highest similiarity of the input images. The prediction code provide a set of predicting results like: “Cabbage – 87%, Carrot 7%, Cucumber - 3%”.
~~~csharp
string PredictLabel(int[] sdrOfInputImage, Dictionary<string, int[]> sdrs)
            {
                //Dictionary<string, List<string>> inputsPath = new Dictionary<string, List<string>>();
                string label = "Could not able to predict the label";
                double similarityWithEachSDR = 0;
                double similarityWithPreviousSDR = 0;
                double temp1 = 0;
                foreach (KeyValuePair<string, List<string>> secondEntry in inputsPath)
                {
                    double sumOfSimilarities = 0; //sum of similarities with images in Same Class(Label)

                    // loop of each folder in input folder
                    var classLabel2 = secondEntry.Key;
                    var filePathList2 = secondEntry.Value;
                    var numberOfImages2 = filePathList2.Count;
                    for (int j = 0; j < numberOfImages2; j++) // loop of each image in each category of inputs
                    {
                        if (!sdrs.TryGetValue(filePathList2[j], out int[] sdr2)) continue;

                        //calculating the similarity between SDR of Input Images with the SDR of the current iterated image (Learning Dataset)
                        similarityWithEachSDR = MathHelpers.CalcArraySimilarity(sdrOfInputImage, sdr2);
                        sumOfSimilarities += Math.Round(similarityWithEachSDR, MidpointRounding.AwayFromZero) ;
                    }
                    //calculating the Average similarity of the Input Image with Learning Images in each Category (Label)
                    sumOfSimilarities /= numberOfImages2;
                    if (sumOfSimilarities > temp1)
                    {
                        temp1 = sumOfSimilarities;
                        label = $"{"The image is predicted as " + secondEntry.Key}";
                        if (temp1 < 50.0) //This depends and selected based on the HTM parameters given in htmconfig.json file
                        {
                            label = "The similarity of Input Image is too low, hence the given image might not belong to the Learning Dataset";
                        }

                    }
                    Console.WriteLine("\n> The Input Image is similar to " + secondEntry.Key + " by " + sumOfSimilarities + " %");
                }
                //Display the highest similarity  of the Input Image with the training category
                Console.WriteLine("\n Highest Similarity is: " + temp1 + " % ");

                return label;
            }
~~~





## RESULTS ACHIEVED
We have conducted tests to find the best correlation matrix and also prediction code has been generated to predict the input labels.
### Case 1: By changing various HTM Parameter to find the best fit correlation Matrix
<img width="470" alt="Fruits360matrix without prediction" src="https://user-images.githubusercontent.com/93146556/158153042-79ae821a-5cea-4cf2-814d-06449932aeab.png">

### Case 2: To Predict the Input Label
<img width="909" alt="fruits360Prediction" src="https://user-images.githubusercontent.com/93146556/158151137-b50a646d-d35b-4a64-90a9-3c78277bc63f.png">

### Case 3: To modify the prediction code to calculate the highest similiarity of the input images

<img width="920" alt="similiarity" src="https://user-images.githubusercontent.com/93146556/158353757-3bf932f8-c7ae-4e44-8d40-8c63b38780c8.png">

<img width="925" alt="RoundPredictionImage" src="https://user-images.githubusercontent.com/93146556/158351849-70edfade-465f-41c3-8166-0e8645e4819d.png">


## WORK IN PROGRESS
- We are conducting more tests to find how other HTM parameter influence the learning.
- To define a parameter to represent the overlapping in order to verify the training process.





