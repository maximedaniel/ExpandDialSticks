import math
import pandas as pd
import neurokit2 as nk
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.pyplot import cm
import os
from scipy import stats
from mystatspackage import statistics, ConfidencePlotter
import sys
from anytree import Node, RenderTree
from utils import *
import warnings
warnings.simplefilter(action='ignore', category=FutureWarning)

# Get the current working directory
cwd = os.getcwd()
imgDir = 'img'
pathToImgDir = os.path.join(cwd, imgDir)
# udo = Node("Udo")
# marc = Node("Marc", parent=udo)
# lian = Node("Lian", parent=marc)
# dan = Node("Dan", parent=udo)
# jet = Node("Jet", parent=dan)
# jan = Node("Jan", parent=dan)
# joe = Node("Joe", parent=dan)

ROOT_NODE_NAME = "FACTORS"
INPUT_COLUMN_NAMES = [ 
    'Date', 'Participant', 'Task', 'Modality', 'Trial' , 
    'Target_X', 'Target_Y',
    'SC_Count_Mean', 'SC_Count_SD', 'SC_Count_Max', 'SC_Count_Min',
    'SC_Amplitude_Mean', 'SC_Amplitude_SD', 'SC_Amplitude_Max', 'SC_Amplitude_Min',
    # eda event-related features
    'EDA_Response_Count',
    'EDA_Response_Mean',
    'EDA_Response_SD',
    'EDA_Response_Max',
    'EDA_Response_Min',

    'EDA_Level_Mean',
    'EDA_Level_SD',
    'EDA_Level_Max',
    'EDA_Level_Min',

     # ppg event-related features
    'BVP_Peak_Count',
    'BVP_Peak_Mean',
    'BVP_Peak_SD',
    'BVP_Peak_Max',
    'BVP_Peak_Min',

    'BVP_Rate_Mean',
    'BVP_Rate_SD',
    'BVP_Rate_Max',
    'BVP_Rate_Min'
    ]
parse_directory = "parse"
quant_file = 'quantitative-description.csv'
df_quant = pd.read_csv(os.path.join(parse_directory, quant_file))
column_name_diff = list(set(df_quant.columns.values.tolist()) - set(INPUT_COLUMN_NAMES))
if len(column_name_diff) != 0:
    err("Missing columns (%s)" %column_name_diff)
    exit()
df_quant['Date'] = pd.to_datetime(df_quant['Date'])
if df_quant.index.duplicated(keep='first').sum(): print("{ERROR] duplicated indexes found!")

TASK_NAMES = ['USERI', 'SYSI']
MODALITY_NAMES = ['USERO', 'SYSO']
CONTROL_NAMES = ['USERO-REST', 'SYSO-REST']
# Removing CONTROL_SESSIONS
df_quant = df_quant.loc[~df_quant['Modality'].isin(CONTROL_NAMES), :]
# [Participant 0] Missing 2/9 trials
# [Participant 6] Missing 6/9 trials
# [Participant 8] No GSR/BVP data to process for trial 2-8
# [Participant 9] No GSR/BVP data to process for trial 7-8
#INCOMPLETE_PARTICIPANTS = [0,6, 8,9]
#df_quant = df_quant.loc[~df_quant['Participant'].isin(INCOMPLETE_PARTICIPANTS), :]
factor_names_list = [['Task', 'Modality']]
factor_types_list = [[str, str]]

# variable_name_list = [
#     'EDA_Response_Mean', 'EDA_Response_Max',
#     'EDA_Level_Mean', 'EDA_Level_Max',
#     'BVP_Peak_Mean', 'BVP_Peak_Max',
#     'BVP_Rate_Mean', 'BVP_Rate_Max'
# ]
variable_name_list = [
    'EDA_Response_Max',
]
for factor_names, factor_types in zip(factor_names_list, factor_types_list):
    multifactor_name = ' x '.join(factor_names)

    factor_values_list = []
    for factor in factor_names:
        factor_values = df_quant[factor].unique().tolist()
        factor_values_list.append(factor_values)
    
    factor_nodes_list = []
    origin_node = Node(ROOT_NODE_NAME)
    for level, factor_values in enumerate(factor_values_list):
        factor_nodes = []
        for factor_value in factor_values:
            if level > 0:
                parent_factor_nodes = factor_nodes_list[level - 1]
                for parent_factor_node in parent_factor_nodes:
                    factor_node = Node(factor_value, parent=parent_factor_node)
                    factor_nodes.append(factor_node)
            else:
                factor_node = Node(factor_value, parent=origin_node)
                factor_nodes.append(factor_node)
        factor_nodes_list.append(factor_nodes)
    for pre, fill, node in RenderTree(origin_node):
        print("%s%s" % (pre, node.name))

    # prepare variable list
    df_variable_list = []
    for variable_name in variable_name_list:
        df_variable_list.append(pd.DataFrame())

    # Iterate over all leafs
    children_factor_nodes = factor_nodes_list[-1]
    for children_factor_node in children_factor_nodes:
        path_to_factor = str(children_factor_node).split("'")[1]
        child_factor_values = path_to_factor.split('/')[2::]
        child_name = '-'.join(child_factor_values)
        df_child = df_quant[eval(" & ".join([
            "(df_quant['{0}'] == '{1}')".format(factor_name, child_factor_value) if factor_type is str
            else "(df_quant['{0}'] == {1})".format(factor_name, child_factor_value)
        for factor_name, factor_type, child_factor_value in zip(factor_names, factor_types, child_factor_values)
        ]))]
        # Fill variable dataframe
        for index, variable_name in enumerate(variable_name_list):
            df_variable = pd.DataFrame(
            columns=[child_name],
            data=df_child[variable_name].values
            )
            df_variable_list[index] = pd.concat([df_variable_list[index], df_variable], axis=1)
    # Perform inferential statistics
    for index, variable_name in enumerate(variable_name_list):
            title = "[%s] %s" %(multifactor_name, variable_name)
            print(title)
            plotImgFile = pathToImgDir + '/' + title + '.png', 
            ConfidencePlotter.ConfidencePlotter(plotImgFile, title, df_variable_list[index], statDf=None)
            exit()
            #statistics.Statistics.quantPaired(pathToImgDir, title, df_variable_list[index], silent=False)
