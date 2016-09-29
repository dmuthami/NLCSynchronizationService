#-------------------------------------------------------------------------------
# Name:        Configurations
# Purpose:
#
# Author:      dmuthami
#
# Created:     16/12/2015
# Copyright:   (c) dmuthami 2014
# Licence:     GPL
#-------------------------------------------------------------------------------
import ConfigParser

#Import global traceback here
import traceback

import logging

#Workspace global variables
Configurations_cat_logfile = ""

#Workspace global variables
Configurations_sdeworkspace = ""
Configurations_gisadminworkspace = ""
Configurations_localauthority = ""
Configurations_connectionfilesfolder = ""
Configurations_reconcilelogfile = ""

#Stand no variables
Configurations_featureClass = ""
Configurations_objectID_fieldName = ""
Configurations_standNo_fieldName = ""
Configurations_local_authority_id_fieldName = ""


def setParameters(configFileLocation):

    #Dirty way of working with global variables
    global Configurations_Config #Set global variable.Usually not favourable in Python
    global Configurations_Sections #Set global variable.Usually not favourable in Python

    Configurations_Config = ConfigParser.ConfigParser() #instantiate ini parser object

    #Read the Config file
    Configurations_Config.read(configFileLocation)

    #Set workspace parameters by calling the below function
    setWorkSpaceParameters()

    # Set stand no parameters
    setStandNoParameters()

    return ""

def setStandNoParameters():
    #read sde (Geodatabase Administrator) user workspace location from Config file location
    global Configurations_featureClass # Needed to modify global copy of C_workspace
    Configurations_featureClass = Configurations_Config.get('StandNo', 'featureClass')

    #read gisadmin (data owner)workspace location from Config file location
    global Configurations_objectID_fieldName # Needed to modify global copy of Configurations_workspaceScratch
    Configurations_objectID_fieldName = Configurations_Config.get('StandNo', 'objectID_fieldName')

    #Get local authority
    global Configurations_standNo_fieldName # Needed to modify global copy of Configurations_workspaceScratch
    Configurations_standNo_fieldName = Configurations_Config.get('StandNo', 'standNo_fieldName')

    #Get folder containing geodatabase connection files
    global Configurations_local_authority_id_fieldName # Needed to modify global copy of Configurations_connectionfilesfolder
    Configurations_local_authority_id_fieldName = Configurations_Config.get('StandNo', 'local_authority_id_fieldName')

    return ""

def setWorkSpaceParameters():
    #read sde (Geodatabase Administrator) user workspace location from Config file location
    global Configurations_sdeworkspace # Needed to modify global copy of C_workspace
    Configurations_sdeworkspace = Configurations_Config.get('Workspace', 'sdeworkspace')

    #read gisadmin (data owner)workspace location from Config file location
    global Configurations_gisadminworkspace # Needed to modify global copy of Configurations_workspaceScratch
    Configurations_gisadminworkspace = Configurations_Config.get('Workspace', 'gisadminworkspace')

    #Get local authority
    global Configurations_localauthority # Needed to modify global copy of Configurations_workspaceScratch
    Configurations_localauthority = Configurations_Config.get('Workspace', 'localauthority')

    #Get folder containing geodatabase connection files
    global Configurations_connectionfilesfolder # Needed to modify global copy of Configurations_connectionfilesfolder
    Configurations_connectionfilesfolder = Configurations_Config.get('Workspace', 'connectionfilesfolder')

    #Get reconcile log file
    global Configurations_reconcilelogfile # Needed to modify global copy of Configurations_reconcilelogfile
    Configurations_reconcilelogfile = Configurations_Config.get('Workspace', 'reconcilelogfile')

    return ""
def main():
    pass

if __name__ == '__main__':
    main()
    #Call function to initialize variables for tool execution
    setParameters(configFileLocation)