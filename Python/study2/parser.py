import os
import time
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import sys
from alive_progress import alive_bar
from utils import *

NB_ROWS = 5
NB_COLS = 6
NB_SESSIONS = 4
NB_TRIALS = 9
plotting = True
printing = False
saving = False

new_user_rotation = False
last_user_rotation = ""
curr_gauge_position = -1

sequence_filename = 'study2-sequence.xlsx'
session_columns = ['rest0', 'session0', 'rest1', 'session1', 'rest2', 'session2', 'rest3', 'session3']
resting_columns = ['rest0', 'rest1', 'rest2', 'rest3']
data_directory = 'data'
parse_directory = 'parse'
log_directory = 'Logs'
physio_directory = 'Physios'

TASK_NAMES = ['SYSI', 'USERI']
MODALITY_NAMES = ['USERO-REST', 'USERO', 'SYSO-REST', 'SYSO']
df_seq = pd.read_excel(sequence_filename, index_col=0)

df_physio_columns = ['PARTICIPANT', 'SESSION', 'TASK', 'MODALITY', 'DATE', 'GSR', 'BVP', 'TMP']
df_log_columns = ['PARTICIPANT','SESSION', 'TASK', 'MODALITY', 'DATE', 
        'TARGET_POSITION', 'TARGET_USER_ROTATION', 'TARGET_SYSTEM_ROTATION',
        'TRIAL_START', 'TRIAL_END', 'SC_START', 'SC_END',
        # left hand and arm transform
        'LEFT_HAND_POSITION_X','LEFT_HAND_POSITION_Y','LEFT_HAND_POSITION_Z','LEFT_HAND_RADIUS' ,
        'LEFT_ARM_POSITION_X','LEFT_ARM_POSITION_Y','LEFT_ARM_POSITION_Z',
        'LEFT_ARM_QUAT_A','LEFT_ARM_QUAT_B', 'LEFT_ARM_QUAT_C', 'LEFT_ARM_QUAT_D', 'LEFT_ARM_RADIUS', 'LEFT_ARM_HEIGHT',
        # right hand and arm transform
        'RIGHT_HAND_POSITION_X', 'RIGHT_HAND_POSITION_Y', 'RIGHT_HAND_POSITION_Z', 'RIGHT_HAND_RADIUS',
        'RIGHT_ARM_POSITION_X', 'RIGHT_ARM_POSITION_Y', 'RIGHT_ARM_POSITION_Z',
        'RIGHT_ARM_QUAT_A', 'RIGHT_ARM_QUAT_B', 'RIGHT_ARM_QUAT_C', 'RIGHT_ARM_QUAT_D',
        'RIGHT_ARM_RADIUS', 'RIGHT_ARM_HEIGHT'
        # pin position
        'PIN_POSITION0', 'PIN_POSITION1', 'PIN_POSITION2', 'PIN_POSITION3', 'PIN_POSITION4',
        'PIN_POSITION5', 'PIN_POSITION6', 'PIN_POSITION7', 'PIN_POSITION8', 'PIN_POSITION9', 
        'PIN_POSITION10', 'PIN_POSITION11', 'PIN_POSITION12', 'PIN_POSITION13', 'PIN_POSITION14',
        'PIN_POSITION15', 'PIN_POSITION16', 'PIN_POSITION17', 'PIN_POSITION18', 'PIN_POSITION19',
        'PIN_POSITION20', 'PIN_POSITION21', 'PIN_POSITION22', 'PIN_POSITION23', 'PIN_POSITION24',
        'PIN_POSITION25', 'PIN_POSITION26', 'PIN_POSITION27', 'PIN_POSITION28', 'PIN_POSITION29'
    ]
df_final_columns =  list(set(df_physio_columns + df_log_columns))
df_final = pd.DataFrame(columns=df_final_columns)
df_final.set_index('DATE', inplace=True)

# output dataframes
nb_participant = len(os.listdir(os.path.join(data_directory, log_directory)))
for participant_index in range(0, nb_participant):
    participant_directory = 'participant%d' %participant_index
    # PROCESS EACH TASK
    for task in TASK_NAMES:
        # PROCESS EACH MODALITY
        for modality in MODALITY_NAMES:
            session_filename = '%s-%s.txt' %(task, modality)
            print("[Participant %d] Parsing %s..." %(participant_index, session_filename))
            isRestSession = True if 'REST' in session_filename else False
            # PHYSIO
            physio_file = open(os.path.join(data_directory, physio_directory, participant_directory, session_filename), 'r')
            KEY_GSR = "E4_Gsr"
            KEY_BVP = "E4_Bvp"
            KEY_TMP = "E4_Temperature"
            df_physio = pd.DataFrame(columns=df_physio_columns)
            lines = physio_file.readlines()
            with alive_bar(len(lines)) as bar:
                for line in lines:
                    splitted_line =  line.strip().split('|')
                    if len(splitted_line) > 1:
                        #t = pd.to_datetime(splitted_line[0], format='%Y-%m-%dT%H:%M:%S.%f')
                        parts = splitted_line[1].split(' ')
                        stamp = pd.Timestamp(float(parts[1].replace(',', '.')), unit='s') 
                        row = {
                            'PARTICIPANT':participant_index, 
                            'SESSION': np.nan,
                            'TASK':task, 
                            'MODALITY': modality,
                            'DATE': stamp,
                            'GSR': np.nan,
                            'BVP': np.nan,
                            'TMP': np.nan
                        }
                        duplicated_index = False
                        if not df_physio[df_physio['DATE'] == stamp].empty:
                            row = df_physio[df_physio['DATE'] == stamp].head(n=1)
                            #print("duplication at row %s" %row.index[0])
                            duplicated_index = True

                        modified_index = False
                        if parts[0] == KEY_GSR:
                            row['GSR'] = float(parts[2].replace(',', '.'))
                            modified_index = True
                        if parts[0] == KEY_BVP:
                            row['BVP'] = float(parts[2].replace(',', '.'))
                            modified_index = True
                        if parts[0] == KEY_TMP:
                            row['TMP'] = float(parts[2].replace(',', '.'))
                            modified_index = True
                        
                        if modified_index:
                            if duplicated_index:
                                df_physio.loc[row.index.values] = row.values
                            else:
                                df_physio = df_physio.append(row, ignore_index=True)
                    bar()
            df_physio.set_index('DATE', inplace=True, verify_integrity=True)
            
            # HANDLE LOG EVENTS
            if isRestSession:
                # FILL EMPTY COLUMN WITH NAN
                for df_log_column in df_log_columns:
                    if df_log_column not in df_physio_columns:
                        df_physio[df_log_column] = np.full(shape=df_physio.shape[0], fill_value=np.nan)
                # ADD FAKE START_TRIAL/END_TRIAL/START_SC/END_SC
                df_physio.loc[df_physio.index[0], 'TRIAL_START'] = 1
                df_physio.loc[df_physio.index[1], 'SC_START'] = 1
                df_physio.loc[df_physio.index[-2], 'SC_END'] = 1
                df_physio.loc[df_physio.index[-1], 'TRIAL_END'] = 1
                df_final = df_final.append(df_physio, ignore_index=False)
            else:
                # LOG
                df_log = pd.DataFrame(columns=df_log_columns)   
                system_file = open(os.path.join(data_directory, log_directory, participant_directory, session_filename), 'r')
                KEY_IDENTITY = "USER_IDENTITY"
                KEY_TRIAL_START = "TRIAL_START"
                KEY_TRIAL_END = "TRIAL_END"
                KEY_TARGET = "TARGET"

                KEY_SC_START = "SYSTEM_TRIGGER_SHAPE_CHANGE"

                KEY_USER_TASK_START = "USER_TASK_START"
                KEY_SYSTEM_TASK_START = "SYSTEM_TASK_START"
                KEY_USER_ROTATION = "USER_ROTATION"
                KEY_USER_TASK_END = "USER_TASK_END"
                KEY_SYSTEM_TASK_END = "SYSTEM_TASK_END"


                KEY_SC_POSITION = "SYSTEM_POSITION"
                KEY_LEFT_HAND = "USER_LEFT_HAND"
                KEY_RIGHT_HAND = "USER_RIGHT_HAND"

                lines = system_file.readlines()
                is_in_trial = False
                with alive_bar(len(lines)) as bar:
                    for line_index, line in enumerate(lines):
                        splitted_line =  line.strip().split('|')
                        if len(splitted_line) > 1:
                            t = pd.to_datetime(splitted_line[0], format='%Y-%m-%dT%H:%M:%S.%f')
                            parts = splitted_line[1].split(' ')
                            row = {
                                'PARTICIPANT':participant_index, 
                                'SESSION':np.nan,
                                'TASK':task, 
                                'MODALITY': modality,
                                'DATE': t, 
                                'TARGET_POSITION': np.nan,
                                'TARGET_USER_ROTATION': np.nan,
                                'TARGET_SYSTEM_ROTATION': np.nan,
                                'TRIAL_START': np.nan,
                                'TRIAL_END': np.nan,
                                'SC_START': np.nan,
                                'SC_END': np.nan,
                                # left hand and arm transform
                                'LEFT_HAND_POSITION_X': np.nan,
                                'LEFT_HAND_POSITION_Y': np.nan,
                                'LEFT_HAND_POSITION_Z': np.nan,
                                'LEFT_HAND_RADIUS' : np.nan,
                                'LEFT_ARM_POSITION_X': np.nan,
                                'LEFT_ARM_POSITION_Y':  np.nan,
                                'LEFT_ARM_POSITION_Z':  np.nan,
                                'LEFT_ARM_QUAT_A': np.nan,
                                'LEFT_ARM_QUAT_B': np.nan,
                                'LEFT_ARM_QUAT_C': np.nan,
                                'LEFT_ARM_QUAT_D': np.nan,
                                'LEFT_ARM_RADIUS': np.nan,
                                'LEFT_ARM_HEIGHT': np.nan,

                                # right hand and arm transform
                                'RIGHT_HAND_POSITION_X': np.nan,
                                'RIGHT_HAND_POSITION_Y': np.nan,
                                'RIGHT_HAND_POSITION_Z': np.nan,
                                'RIGHT_HAND_RADIUS' : np.nan,
                                'RIGHT_ARM_POSITION_X': np.nan,
                                'RIGHT_ARM_POSITION_Y':  np.nan,
                                'RIGHT_ARM_POSITION_Z':  np.nan,
                                'RIGHT_ARM_QUAT_A': np.nan,
                                'RIGHT_ARM_QUAT_B': np.nan,
                                'RIGHT_ARM_QUAT_C': np.nan,
                                'RIGHT_ARM_QUAT_D': np.nan,
                                'RIGHT_ARM_RADIUS': np.nan,
                                'RIGHT_ARM_HEIGHT': np.nan,

                                # pin position
                                'PIN_POSITION0': np.nan,
                                'PIN_POSITION1': np.nan,
                                'PIN_POSITION2': np.nan,
                                'PIN_POSITION3': np.nan,
                                'PIN_POSITION4': np.nan,
                                'PIN_POSITION5': np.nan,
                                'PIN_POSITION6': np.nan,
                                'PIN_POSITION7': np.nan,
                                'PIN_POSITION8': np.nan,
                                'PIN_POSITION9': np.nan,
                                'PIN_POSITION10': np.nan,
                                'PIN_POSITION11': np.nan,
                                'PIN_POSITION12': np.nan,
                                'PIN_POSITION13': np.nan,
                                'PIN_POSITION14': np.nan,
                                'PIN_POSITION15': np.nan,
                                'PIN_POSITION16': np.nan,
                                'PIN_POSITION17': np.nan,
                                'PIN_POSITION18': np.nan,
                                'PIN_POSITION19': np.nan,
                                'PIN_POSITION20': np.nan,
                                'PIN_POSITION21': np.nan,
                                'PIN_POSITION22': np.nan,
                                'PIN_POSITION23': np.nan,
                                'PIN_POSITION24': np.nan,
                                'PIN_POSITION25': np.nan,
                                'PIN_POSITION26': np.nan,
                                'PIN_POSITION27': np.nan,
                                'PIN_POSITION28': np.nan,
                                'PIN_POSITION29': np.nan,
                                }
                                
                            # Check for duplicated index
                            duplicated_index = False
                            if not df_log[df_log['DATE'] == t].empty:
                                row = df_log[df_log['DATE'] == t].head(n=1)
                                duplicated_index = True
                            
                            modified_index = False

                            if parts[0] == KEY_IDENTITY:
                                modality_name = splitted_line[2].replace(' ', '').replace('TEM', '')
                                task_name = splitted_line[3].replace(' ', '').replace('TEM', '')
                                if task != task_name or modality != modality_name:
                                    err("Looked for session '%s-%s' but found '%s-%s'" %( task, modality, task_name, modality_name))
                            
                            if parts[0] == KEY_TRIAL_START:
                                is_in_trial = True
                                row['TRIAL_START'] = 1
                                modified_index = True

                            if parts[0] == KEY_SC_START:
                                row['SC_START'] = 1
                                modified_index = True

                            if is_in_trial and (parts[0] == KEY_TRIAL_END or line_index == len(lines) - 1):
                                is_in_trial = False
                                row['TRIAL_END'] = 1
                                row['SC_END'] = 1
                                modified_index = True

                            
                            if parts[0] == KEY_USER_ROTATION or parts[0] == KEY_USER_TASK_START or parts[0] == KEY_USER_TASK_END or parts[0] == KEY_SYSTEM_TASK_START or parts[0] == KEY_SYSTEM_TASK_END:
                                r =  float(parts[1].replace(',','').replace('(', '').replace(')',''))
                                c =  float(parts[2].replace(',','').replace('(', '').replace(')',''))
                                row['TARGET_POSITION'] = r * NB_COLS + c
                                cadran = float(parts[4].replace(',','').replace('(', '').replace(')',''))
                                row['TARGET_SYSTEM_ROTATION'] = cadran
                                aiguille = float(parts[6].replace(',','').replace('(', '').replace(')',''))
                                row['TARGET_USER_ROTATION'] = aiguille
                                modified_index = True

                            if parts[0] == KEY_TARGET:
                                r =  float(parts[1].replace(',','').replace('(', '').replace(')',''))
                                c =  float(parts[2].replace(',','').replace('(', '').replace(')',''))
                                row['TARGET_POSITION'] = r * NB_COLS + c
                                modified_index = True

                            if parts[0] == KEY_SC_POSITION:
                                # Fill pin position rows
                                sc_positions = pd.Series(parts[1:]).astype(int)
                                for i in sc_positions.index.values:
                                    row["PIN_POSITION%s" %i] = sc_positions.values[i]
                                modified_index = True

                            if parts[0] == KEY_RIGHT_HAND and len(parts) > 1:
                                row['RIGHT_HAND_POSITION_X'] = float(parts[4].replace(',','').replace('(', '').replace(')',''))
                                row['RIGHT_HAND_POSITION_Y'] = float(parts[5].replace(',','').replace('(', '').replace(')',''))
                                row['RIGHT_HAND_POSITION_Z'] = float(parts[6].replace(',','').replace('(', '').replace(')',''))
                                row['RIGHT_HAND_RADIUS'] = float(parts[8].replace(',','.'))

                                row['RIGHT_ARM_POSITION_X'] = float(parts[12].replace(',','').replace('(', '').replace(')',''))
                                row['RIGHT_ARM_POSITION_Y'] = float(parts[13].replace(',','').replace('(', '').replace(')',''))
                                row['RIGHT_ARM_POSITION_Z'] = float(parts[14].replace(',','').replace('(', '').replace(')',''))
                                
                                row['RIGHT_ARM_QUAT_A'] = float(parts[16].replace(',','').replace('(', '').replace(')',''))
                                row['RIGHT_ARM_QUAT_B'] = float(parts[17].replace(',','').replace('(', '').replace(')',''))
                                row['RIGHT_ARM_QUAT_C'] = float(parts[18].replace(',','').replace('(', '').replace(')',''))
                                row['RIGHT_ARM_QUAT_D'] = float(parts[19].replace(',','').replace('(', '').replace(')',''))
                                
                                row['RIGHT_ARM_RADIUS'] = float(parts[21].replace(',','.'))

                                row['RIGHT_ARM_HEIGHT'] = float(parts[23].replace(',','.'))
                                
                                modified_index = True

                            if parts[0] == KEY_LEFT_HAND and len(parts) > 1:
                                row['LEFT_HAND_POSITION_X'] = float(parts[4].replace(',','').replace('(', '').replace(')',''))
                                row['LEFT_HAND_POSITION_Y'] = float(parts[5].replace(',','').replace('(', '').replace(')',''))
                                row['LEFT_HAND_POSITION_Z'] = float(parts[6].replace(',','').replace('(', '').replace(')',''))
                                row['LEFT_HAND_RADIUS'] = float(parts[8].replace(',','.'))

                                row['LEFT_ARM_POSITION_X'] = float(parts[12].replace(',','').replace('(', '').replace(')',''))
                                row['LEFT_ARM_POSITION_Y'] = float(parts[13].replace(',','').replace('(', '').replace(')',''))
                                row['LEFT_ARM_POSITION_Z'] = float(parts[14].replace(',','').replace('(', '').replace(')',''))
                                
                                row['LEFT_ARM_QUAT_A'] = float(parts[16].replace(',','').replace('(', '').replace(')',''))
                                row['LEFT_ARM_QUAT_B'] = float(parts[17].replace(',','').replace('(', '').replace(')',''))
                                row['LEFT_ARM_QUAT_C'] = float(parts[18].replace(',','').replace('(', '').replace(')',''))
                                row['LEFT_ARM_QUAT_D'] = float(parts[19].replace(',','').replace('(', '').replace(')',''))
                                
                                row['LEFT_ARM_RADIUS'] = float(parts[21].replace(',','.'))
                                
                                row['LEFT_ARM_HEIGHT'] = float(parts[23].replace(',','.'))
                                
                                modified_index = True
                                
                            if modified_index:
                                if duplicated_index:
                                    df_log.loc[row.index.values] = row.values
                                else:
                                    df_log = df_log.append(row, ignore_index=True)
                            bar()
                df_log.set_index('DATE', inplace=True,  verify_integrity=True)
                #Concatenate physio and log data and interpolate them.
                df_concat = pd.concat([df_physio, df_log])
                df_concat = df_concat.sort_index()
                # CHECK FOR DUPLICATED INDEXES AFTER MERGE
                duplicated_concat_indexes = df_concat.index.duplicated(keep='last')
                if duplicated_concat_indexes.sum() != 0:
                    print("Duplicates detected! Keeping last occurency...")
                    print(df_concat[duplicated_concat_indexes])
                    df_concat[duplicated_concat_indexes].to_csv("duplicated_index_"+str(time.time()).replace('.', '_') + ".csv")
                    df_concat = df_concat[~duplicated_concat_indexes]
                
                # assert df_concat to debug
                nb_trial_start = df_concat[df_concat['TRIAL_START'] == 1].shape[0]
                nb_trial_end  = df_concat[df_concat['TRIAL_END'] == 1].shape[0]
                nb_sc_start = df_concat[df_concat['SC_START'] == 1].shape[0]
                nb_sc_end  = df_concat[df_concat['SC_END'] == 1].shape[0]
                if nb_trial_start != 9:
                    warn("Found %s/%s start trials (missing %s)" %(nb_trial_start, NB_TRIALS, NB_TRIALS-nb_trial_start))
                if nb_trial_end != 9:
                    warn("Found %s/%s end trials (missing %s)" %(nb_trial_end, NB_TRIALS, NB_TRIALS-nb_trial_end))
                if nb_sc_start != 9:
                    warn("Found %s/%s start sc (missing %s)" %(nb_sc_start, NB_TRIALS, NB_TRIALS-nb_sc_start))
                if nb_sc_end != 9:
                    warn("Found %s/%s end sc (missing %s)" %(nb_sc_end, NB_TRIALS, NB_TRIALS-nb_sc_end))


                trial_start_indexes = df_concat[df_concat['TRIAL_START'] == 1].index
                trial_end_indexes = df_concat[df_concat['TRIAL_END'] == 1].index
                trial_i = 0
                for(trial_start_index, trial_end_index) in zip(trial_start_indexes, trial_end_indexes):
                    df_trial = df_concat.loc[trial_start_index:trial_end_index]
                    nb_gsr = df_trial[~np.isnan(df_trial['GSR'])].shape[0]
                    nb_bvp = df_trial[~np.isnan(df_trial['BVP'])].shape[0]
                    nb_tmp = df_trial[~np.isnan(df_trial['TMP'])].shape[0]
                    if nb_gsr == 0 or nb_bvp == 0 or nb_tmp == 0:
                        warn("Lost physio data for trial %s (%s GRS found, %s BVP found, %s TMP found)" % (trial_i, nb_gsr, nb_bvp, nb_tmp))
                    trial_i += 1
                df_final = df_final.append(df_concat, ignore_index=False)
    
# Save all data
df_final.to_csv(os.path.join(parse_directory, 'all.csv'))