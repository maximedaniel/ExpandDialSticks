from tracemalloc import Statistic
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
from mystats import statistics
import sys
from anytree import Node, RenderTree
import warnings
warnings.simplefilter(action='ignore', category=FutureWarning)

# Get the current working directory
cwd = os.getcwd()
imgDir = 'img'
pathToImgDir = os.path.join(cwd, imgDir)

ROOT_NODE_NAME = "FACTORS"

data_directory = 'data'
form_directory = 'Forms/all'
demography_filename = "demography.xlsx"
demography_headers = ['DATE', 'AGE', 'SEXE', 'STUDY', 'HAND', 'EYE', 'COBOT']
df_demography = pd.read_excel(os.path.join(data_directory,form_directory, demography_filename ), names=demography_headers)
# df_demography = df_demography.drop(columns=['DATE', 'AGE_TO_DELETE', 'COBOT_TO_DELETE'])
# df_demography = df_demography.drop(index=[0, 1, 2])
df_demography.reset_index(inplace=True, drop=True)
print(df_demography.describe())
for column_name in df_demography.columns.values:
    print(df_demography[column_name].value_counts())

ages = np.array([27, 27, 27, 27, 22, 22, 43, 21, 29, 40, 38, 26, 33, 32, 34, 37])
print("%f+/-%f" %(ages.mean(), ages.std()))
sequence_filename = 'study2-sequence.xlsx'
stress_filename = "stress-and-safety.xlsx"
form_columns = ['session0', 'session1', 'session2', 'session3']
header_names = ['DATE',
                'INSECURE⇔SECURE', 'ANXIOUS⇔RELAXED', 'UNCOMFORTABLE⇔COMFORTABLE', 'LACK_CONTROL⇔IN_CONTROL', 'THREATENING⇔SAFE', 'UNFAMILIAR⇔FAMILIAR', 'UNRELIABLE⇔RELIABLE', 'SCARY⇔CALMING',
                'OBSTRUCTIVE⇔SUPPORTIVE', 'COMPLICATED⇔EASY', 'INEFFICIENT⇔EFFICIENT', 'CONFUSING⇔CLEAR', 'BORING⇔EXCITING', 'NOT_INTERESTING⇔INTERESTING', 'CONVENTIONAL⇔INVENTIVE', 'USUAL⇔LEADING_EDGE',
                'UNPREDICTABLE⇔PREDICTABLE', 'EGOISTIC⇔ALTRUISTIC', 'PROTECTING_ROBOT⇔PROTECTING_USER']
df_qual = pd.read_excel(os.path.join(data_directory,form_directory, stress_filename ), names=header_names)

df_seq = pd.read_excel(sequence_filename, index_col=0)
participants_indexes = []
tasks_indexes = []
modalities_indexes = []

for index, row in df_qual.iterrows():
    session_index = int(index % 4)
    participant_index = int(index / 4)
    #print("%s:%s" %(participant_index, session_index))
    sequence_tag = df_seq.iloc[participant_index][session_index]
    task = sequence_tag.split('-')[0]
    modality = sequence_tag.split('-')[1]
    participants_indexes.append(participant_index)
    tasks_indexes.append(task)
    modalities_indexes.append(modality)
df_qual.insert(1, "Modality", modalities_indexes)
df_qual.insert(1, "Task", tasks_indexes)
df_qual.insert(1, "Participant", participants_indexes)

# Compute UEQ-S 
# ueqs_score_headers = ["HEDONIC", "PRAGMATIC", "OVERALL"]

# for row i ndf_qual:
#     print(row)
#     assert 0
# get all ueq-s items for the current leaf
# print(child_name)
# df_items_ueqs = df_child.loc[:, all_quality_list]
# df_items_ueqs_signed = df_items_ueqs - 4
# df_skale_means_per_person = pd.DataFrame(columns=['Pragmatic Quality', 'Hedonic Quality', 'Overall'])
# df_skale_means_per_person['Pragmatic Quality'] = df_items_ueqs_signed.loc[:, pragmatic_quality_list].mean(axis=1)
# df_skale_means_per_person['Hedonic Quality'] = df_items_ueqs_signed.loc[:, hedonic_quality_list].mean(axis=1)
# df_skale_means_per_person['Overall'] = df_items_ueqs_signed.loc[:, all_quality_list].mean(axis=1)
# df_skale_means_per_person = df_skale_means_per_person.reset_index()
# print(df_skale_means_per_person)
# df_item_ueqs_desc = statistics.Statistics.describePlus(df_items_ueqs_signed)
# df_skale_means_per_person_desc = statistics.Statistics.describePlus(df_skale_means_per_person)
# print(df_skale_means_per_person_desc)

# df_user_sms = df_stress[(df_stress['TASK'] == 'USER') & (df_stress['MODALITY'] == 'SMS')]
# df_user_ssm = df_stress[(df_stress['TASK'] == 'USER') & (df_stress['MODALITY'] == 'SSM')]
# df_system_sms = df_stress[(df_stress['TASK'] == 'SYSTEM') & (df_stress['MODALITY'] == 'SMS')]
# df_system_ssm = df_stress[(df_stress['TASK'] == 'SYSTEM') & (df_stress['MODALITY'] == 'SSM')]
factor_names_list = [ ['Task', 'Modality']]
factor_types_list = [[str, str]]

variable_name_list = [
#'INSECURE⇔SECURE', 'ANXIOUS⇔RELAXED', 'UNCOMFORTABLE⇔COMFORTABLE', 'LACK_CONTROL⇔IN_CONTROL',
#'THREATENING⇔SAFE', 'UNFAMILIAR⇔FAMILIAR', 'UNRELIABLE⇔RELIABLE', 'SCARY⇔CALMING',
'OBSTRUCTIVE⇔SUPPORTIVE', 'COMPLICATED⇔EASY', 'INEFFICIENT⇔EFFICIENT', 'CONFUSING⇔CLEAR', 
'BORING⇔EXCITING', 'NOT_INTERESTING⇔INTERESTING', 'CONVENTIONAL⇔INVENTIVE', 'USUAL⇔LEADING_EDGE',
#'UNPREDICTABLE⇔PREDICTABLE', 'EGOISTIC⇔ALTRUISTIC', 'PROTECTING_ROBOT⇔PROTECTING_USER'
]

variable_scale_list = [
    5, 5, 5,
    5, 5, 5,
    5, 5, 7,
    7, 7, 7,
    7, 7, 7,
    7, 5, 5,
    5
]
for factor_names, factor_types in zip(factor_names_list, factor_types_list):
    multifactor_name = ' x '.join(factor_names)

    factor_values_list = []
    for factor in factor_names:
        factor_values = df_qual[factor].unique().tolist()
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
        df_child = df_qual[eval(" & ".join([
            "(df_qual['{0}'] == '{1}')".format(factor_name, child_factor_value) if factor_type is str
            else "(df_qual['{0}'] == {1})".format(factor_name, child_factor_value)
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
            statistics.Statistics.qualOrdinalPaired(pathToImgDir, title, df_variable_list[index], variable_scale_list[index], silent=False)
