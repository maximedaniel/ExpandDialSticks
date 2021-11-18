import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
import statistics
import sys

data_directory = 'data'
form_directory = 'Forms/all'
demography_filename = "demography.xlsx"
demography_headers = ['DATE','AGE_TO_DELETE', 'SEXE', 'STUDY', 'AGE', 'COBOT', 'COBOT_TO_DELETE']
df_demography = pd.read_excel(os.path.join(data_directory,form_directory, demography_filename ), names=demography_headers)
df_demography = df_demography.drop(columns=['DATE', 'AGE_TO_DELETE', 'COBOT_TO_DELETE'])
df_demography = df_demography.drop(index=[0, 1, 2])
df_demography.reset_index(inplace=True, drop=True)
for column_name in df_demography.columns.values:
    print(df_demography[column_name].value_counts())



sequence_filename = 'sequence.xlsx'
stress_filename = "stress-and-safety.xlsx"
form_columns = ['session0', 'session1', 'session2', 'session3']
header_names = ['DATE', 'CALM⇔STRESSED', 'NERVOUS⇔RELAXED', 'CALM⇔AGITATED', 'SERENE⇔SURPRISED', 'SLOW⇔FAST', 'RIGID⇔FLUID', 'CALM⇔BRUTAL', 'PREDICTABLE⇔SURPRISING', 'SYNCHRONOUS⇔ASYNCHRONOUS']
df_stress = pd.read_excel(os.path.join(data_directory,form_directory, stress_filename ), names=header_names)
valence_names = ['CALM⇔STRESSED', 'NERVOUS⇔RELAXED', 'CALM⇔AGITATED', 'SERENE⇔SURPRISED', 'SLOW⇔FAST', 'RIGID⇔FLUID', 'CALM⇔BRUTAL', 'PREDICTABLE⇔SURPRISING', 'SYNCHRONOUS⇔ASYNCHRONOUS']
valence_scales = [7,5,5,5,5,5,5,5,5]

df_seq = pd.read_excel(sequence_filename, index_col=0)
participants_indexes = []
tasks_indexes = []
modalities_indexes = []

for index, row in df_stress.iterrows():
    session_index = int(index % 4)
    participant_index = int(index / 4)
    #print("%s:%s" %(participant_index, session_index))
    sequence_tag = df_seq.iloc[participant_index][session_index]
    task = sequence_tag.split('-')[0]
    modality = sequence_tag.split('-')[1]
    participants_indexes.append(participant_index)
    tasks_indexes.append(task)
    modalities_indexes.append(modality)
df_stress.insert(1, "MODALITY", modalities_indexes)
df_stress.insert(1, "TASK", tasks_indexes)
df_stress.insert(1, "PARTICIPANT", participants_indexes)
#df_stress = df_stress.drop(columns=['DATE', 'PARTICIPANT'])
# export data
# df_stress['MODALITY'] = df_stress['TASK'] + '-' + df_stress['MODALITY']
# df_stress = df_stress.drop(columns=['DATE', 'TASK'])
# df_stress.to_excel('expandialsticks-study1-qualitative-data.xlsx', index=False)
# 
# df_user_sms = df_stress[(df_stress['TASK'] == 'USER') & (df_stress['MODALITY'] == 'SMS')]
# df_user_ssm = df_stress[(df_stress['TASK'] == 'USER') & (df_stress['MODALITY'] == 'SSM')]
# df_system_sms = df_stress[(df_stress['TASK'] == 'SYSTEM') & (df_stress['MODALITY'] == 'SMS')]
# df_system_ssm = df_stress[(df_stress['TASK'] == 'SYSTEM') & (df_stress['MODALITY'] == 'SSM')]

for valence_name, valence_scale in zip(valence_names, valence_scales):
    df_valence = df_stress[['TASK', 'MODALITY', valence_name]]
    # df_valence.insert(0, 'USER-SMS', df_user_sms[valence_name].values)
    # df_valence.insert(0, 'USER-SSM', df_user_ssm[valence_name].values)
    # df_valence.insert(0, 'SYSTEM-SMS', df_system_sms[valence_name].values)
    # df_valence.insert(0, 'SYSTEM-SSM', df_system_ssm[valence_name].values)
    df_valence.loc[:, valence_name] = df_valence[valence_name].sub(1)
    # meltedSheetDf = pd.melt(df_valence,var_name='columns', value_name='index')
    # print(meltedSheetDf)
    # sys.exit()
    statistics.Statistics.qualOrdinalPaired(valence_name, df_valence, valence_scale)