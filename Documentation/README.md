# ML21/22-1.2.Analyse Image Classification (Fruits 360 dataset) : CodeChasers
Team members: Anu Maria Varghese, Tiniya Vinod Puthanpurayil, Veena Alphonsa Jose

# **Project Description**
## **AIM**
In the previous work at the university an Image Classification solution has been implemented. Our task is to implement a program that uses the existing solution as a library and start a training of learning images. The image dataset we have used in this project is Fruits 360 dataset images. Our task is to change various learning parameters and to find the best fit that shows image classification. Most important learning parameters are: Global/Local Inhibition, Potential Radius, Local Area Density and NumofActiveColumnsPerInArea. We have to demonstrate how these parameters influence the learning. Our code must provide the learning code and prediction code. After learning user should use your code and test the quality of learning. For example, the user after learning enter the image “table”. The prediction code provide a set of predicting results like: “Table – 87%, chair 7%, light - 3%”.

## METHODS
### 1 Prepare the program's directory:
First we have prepared images that are required for the training. Images must be copied in the following folder structure along with the application and the config json:  

 ![](Images/WorkingDirectory.png)
 
The imagesets are stored inside "InputFolder/".  

![](Images/InputFolder.png)  

Each Imageset is stored inside a folder whose name is the set's label.  

![](Images/SampleData.png)  

 Sample input folder of the project can be found [here](ImageClassification/ImageClassification/InputFolder)  
 ### 2 Start the application by passing required command line arguments
 ~~~
 ImageClassifier -if "InputFolder" -cf htmconfig.json
 ~~~
 -if   "Input Images folder path"  
 -cf   "json htm config file path"  
 
 **HTM Configuration**  
 HTM setting of the project can be inputted to the program by means of a .json file [htmconfig.json](ImageClassification/ImageClassification/htmconfig.json).  
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

![Sample output of the experiment after learning](Images/OutputExample.png)  
The Images used was collected from [Fruit 360](https://www.kaggle.com/moltean/fruits).  

#### (4) How to run the application in Visual Studio
Visual Studio can add arguments (args) parameter to run your code.  
![](Images/LaunchProfile.png)
This is done by changing the arguments command line arguments in Debug Properties Launch Profiles to 
~~~
-cf htmconfig.json -if "InputFolder"
~~~
-cf add the option of the configuration file "htmconfig.json"  
-if add the option of the training Input Folder "InputFolder/".  
This folder contains folders of images, where the folder names also act as the label for the images inside it.  

## APPROACH
#### 1. By changing various HTM Parameter to find the best fit correlation Matrix
Our task is to change various learning parameters and to find the best fit that shows image classification. Most important learning parameters are: Global/Local Inhibition, Potential Radius, Local Area Density and NumofActiveColumnsPerInArea and we found how these parameters influenced learning. After conducting various tests we have been able to find the parameters at which we get the least overlapping inbetween Micro and Macro and thus the best correlation matrix.


#### 2. To Predict the Input Label
We have compared the SDRs of the input label with the SDRs of the existing dataset and predicted the input label. The prediction code will give the name of the label which is being predicted with the highest similiarity. 

## RESULTS ACHIEVED
We have conducted tests to find the best correlation matrix and also prediction code has been generated to predict the input labels.

## WORK IN PROGRESS
- We are conducting more tests to find how other HTM parameter influence the learning.
- To define a parameter to represent the overlapping in order to verify the training process.



