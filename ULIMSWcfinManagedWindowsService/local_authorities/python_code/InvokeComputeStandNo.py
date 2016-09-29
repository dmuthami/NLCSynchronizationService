import os, sys
import logging
import arcpy, time, smtplib
import traceback
from arcpy import env
from datetime import datetime

##Custom module containing functions
import Configurations
import Compute_Stand_No

#Set-up logging
logger = logging.getLogger('myapp')
Configurations.Configurations_localauthority = "temp"
Configurations.Configurations_cat_logfile = os.path.join(os.path.dirname(__file__), Configurations.Configurations_localauthority+'.log')
hdlr = logging.FileHandler(Configurations.Configurations_cat_logfile)
formatter = logging.Formatter('%(asctime)s %(levelname)s %(message)s')
hdlr.setFormatter(formatter)
logger.addHandler(hdlr)
logger.setLevel(logging.INFO)

#start time
msg ="\n---------------------------------Compute Stand No---------------------------------\n"+"Start Time : " + datetime.now().strftime("-%y-%m-%d_%H-%M-%S")+ "\n------------------------------------------------------------------\n"
print msg
#Logging
logger.info(msg)

try:
    #Move from current working directory one level up
    os.chdir("..")

    #Store current directory of interest for use later
    currDir = os.getcwd()

    ##Obtain script parameter values
    ##location for configuration file
    ##Acquire it as a parameter either from terminal, console or via application
    configFileLocation=arcpy.GetParameterAsText(0)#Get from console or GUI being user input
    if configFileLocation =='': #Checks if supplied parameter is null
        #Defaults to below hard coded path if the parameter is not supplied. NB. May throw exceptions if it defaults to path below
        # since path below might not  be existing in your system with the said file name required
        configFileLocation=os.path.join(currDir, 'okahandja','Config.ini')

    ##Read from config file
    #If for any reason an exception is thrown here then subsequent code will not be executed
    Configurations.setParameters(configFileLocation)

    #Write to the log file specifying the localauthority
    msg = "\n Local Authority : "+Configurations.Configurations_localauthority
    logger.info(msg)

    #Write to the console specifying the localauthority
    print msg

    #output location of Config file
    logger.info("\n Config File Path : "+configFileLocation)

    ##Proper Code starts here
    #Overwrite output
    env.overwriteOutput = True

    null = ""
    currDirDotNet=arcpy.GetParameterAsText(2)#Get from console or GUI being user input passed from .Net application
    if currDirDotNet is not null: #Checks if supplied parameter is null
        #Get path from system argument for location of reconcile log file
        currDir = currDirDotNet
        #Output path for currDir
        msg=  "\n"+currDirDotNet+"\n"
        logger.info("currDirDotNet path : "+msg)
        #print msg
    else:
        #Output path for currDir
        msg=  "\n"+currDir+"\n"
        logger.info("currDir path : "+msg)
        #print msg

    # Get the Connection to the geodatabase administrator
    connSDE = os.path.join(currDir,\
    Configurations.Configurations_localauthority,\
    Configurations.Configurations_connectionfilesfolder,\
    Configurations.Configurations_sdeworkspace)

    #Output path for connSDE
    msg=  "\n"+connSDE+"\n"
    logger.info("ConnSDE path : "+msg)
    #print msg

    # Get Connection to the geodatabase as the data owner
    connGISADMIN = os.path.join(currDir,\
    Configurations.Configurations_localauthority,\
    Configurations.Configurations_connectionfilesfolder,\
    Configurations.Configurations_gisadminworkspace)

    # Get path to reconcile log file path
    reconcilelogfile =   os.getcwd() + "\\"+ Configurations.Configurations_localauthority +"\\"+\
     Configurations.Configurations_localauthority+"_" + Configurations.Configurations_reconcilelogfile

    dotnetReconcileLogFile=arcpy.GetParameterAsText(1)#Get from console or GUI being user input passed from .Net application
    if dotnetReconcileLogFile is not null: #Checks if supplied parameter is null
        #Get path from system argument for location of reconcile log file
        reconcilelogfile = dotnetReconcileLogFile

    #output location of reconcile log file
    logger.info("\n Reconcile Log File Path : "+ reconcilelogfile)

    ### Set Parameters for the Compute Stand No Module
    ### ---------------------------------------------
    ###---------------------------------------------


    # set the workspace
    env.workspace = connSDE

    #workspace variable
    wrkspc = env.workspace

    ##--------------------------------------------------
    ##-----Allow connections to the geodatabase. Previuos code executions code have
    ##-------executed unsuccessfully leading to the fact that the geodatabase is a state that cannot accept connections
    ##-------Ensures no manual intervention via creationof geodatabase connection as DBA to allow connections is NEEDED
    ##----------------------------------------------------------

    #Accept new connections to the database.
    arcpy.AcceptConnections(connSDE, True)


    ##----------------------------------------------------------
    ##-------------------------Call code to compute stand number
    ##----------------------------------------------------------
    print "\n Data owner(GIS Admin) connection file path : "+connGISADMIN
    print "\n Reference Number/Stand No : "+Configurations.Configurations_standNo_fieldName
    print "\n Feature Class to compute stand number : "+Configurations.Configurations_featureClass
    print "\n Object ID Field Name : "+Configurations.Configurations_objectID_fieldName
    print "\n Local Authority ID Field Name : "+Configurations.Configurations_local_authority_id_fieldName

    ##-----------Set workspace again to GIS admin so as to compute stand no
    ##------------

    env.workspace = connGISADMIN

    ## Call to compute stand no
    ##------------------------
    Compute_Stand_No.mainMethod(connGISADMIN,\
    Configurations.Configurations_featureClass,
    Configurations.Configurations_objectID_fieldName,\
    Configurations.Configurations_standNo_fieldName,\
    Configurations.Configurations_local_authority_id_fieldName)



    msg ="\nExited Computing Stand No In Spectacular Success \n-------------Compute Stand No-----------------------------------------------------\n"+"End Time : " + datetime.now().strftime("-%y-%m-%d_%H-%M-%S")+ "\n------------------------------------------------------------------\n"

    print msg
    #Logging
    logger.info(msg)
except:
    ## Return any Python specific errors and any error returned by the geoprocessor
    ##
    tb = sys.exc_info()[2]
    tbinfo = traceback.format_tb(tb)[0]
    pymsg = "PYTHON ERRORS:\nTraceback Info:\n" + tbinfo + "\nError Info:\n    " + \
            str(sys.exc_type)+ ": " + str(sys.exc_value) + "\n"
    msgs = "Geoprocessing  Errors :\n" + arcpy.GetMessages(2) + "\n"

    ##Add custom informative message to the Python script tool
    arcpy.AddError(pymsg) #Add error message to the Python script tool(Progress dialog box, Results windows and Python Window).
    arcpy.AddError(msgs)  #Add error message to the Python script tool(Progress dialog box, Results windows and Python Window).

    ##For debugging purposes only
    ##To be commented on python script scheduling in Windows _log
    print pymsg
    print "\n" +msgs
    logger.info("InvokeComputeStandNo.py "+pymsg)
    logger.info("InvokeComputeStandNo.py "+msgs)
    msg ="\nFailed \n-------------------------------Compute Stand No-----------------------------------\n"+"End Time : " + datetime.now().strftime("-%y-%m-%d_%H-%M-%S")+ "\n------------------------------------------------------------------\n"
    print msg

    #Logging
    logger.info(msg)
