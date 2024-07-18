import os
import time
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import sys
from alive_progress import alive_bar
from plotter.StackedBarPlotter import StackedBarPlotter 
from utils import *
import warnings
warnings.simplefilter(action='ignore', category=FutureWarning)

NB_SESSIONS = 4
NB_TRIALS = 9
plotting = True
printing = False
saving = False

new_user_rotation = False
last_user_rotation = ""
curr_gauge_position = -1

sequence_filename = 'sequence.xlsx'
session_columns = ['rest0', 'session0', 'rest1', 'session1', 'rest2', 'session2', 'rest3', 'session3']
resting_columns = ['rest0', 'rest1', 'rest2', 'rest3']
data_directory = 'data'
parse_directory = 'parse'
log_directory = 'Logs'
physio_directory = 'Physios'

TASK_NAMES = ['SYSTEM', 'USER']
MODALITY_NAMES = ['SMS', 'SSM', 'SMS-REST', 'SSM-REST']
df_seq = pd.read_excel(sequence_filename, index_col=0)

df_physio_columns = ['PARTICIPANT', 'SESSION', 'TASK', 'MODALITY', 'DATE', 'GSR', 'BVP', 'TMP']
df_log_columns = ['PARTICIPANT', 'SESSION', 'TASK', 'MODALITY', 'DATE', 'TARGET_POSITION',
                'TARGET_ORIENTATION_X','TARGET_ORIENTATION_Y', 'TARGET_USER_ROTATION',
                'TARGET_SYSTEM_ROTATION', 'TRIAL_START', 'TRIAL_END', 'SC_START', 'SC_END',
                # left hand position and radius,
                'LEFT_HAND_POSITION_X', 'LEFT_HAND_POSITION_Y', 'LEFT_HAND_POSITION_Z',
                'RIGHT_HAND_POSITION_X', 'RIGHT_HAND_POSITION_Y', 'RIGHT_HAND_POSITION_Z',
                # pin position
                'PIN_POSITION0', 'PIN_POSITION1', 'PIN_POSITION2', 'PIN_POSITION3', 'PIN_POSITION4',
                'PIN_POSITION5', 'PIN_POSITION6', 'PIN_POSITION7', 'PIN_POSITION8', 'PIN_POSITION9', 
                'PIN_POSITION10', 'PIN_POSITION11', 'PIN_POSITION12', 'PIN_POSITION13', 'PIN_POSITION14',
                'PIN_POSITION15', 'PIN_POSITION16', 'PIN_POSITION17', 'PIN_POSITION18', 'PIN_POSITION19',
                'PIN_POSITION20', 'PIN_POSITION21', 'PIN_POSITION22', 'PIN_POSITION23', 'PIN_POSITION24',
                'PIN_POSITION25', 'PIN_POSITION26', 'PIN_POSITION27', 'PIN_POSITION28', 'PIN_POSITION29',
                # pin proximity
                'PIN_PROXIMITY0', 'PIN_PROXIMITY1', 'PIN_PROXIMITY2', 'PIN_PROXIMITY3','PIN_PROXIMITY4',
                'PIN_PROXIMITY5', 'PIN_PROXIMITY6', 'PIN_PROXIMITY7', 'PIN_PROXIMITY8', 'PIN_PROXIMITY9',
                'PIN_PROXIMITY10', 'PIN_PROXIMITY11', 'PIN_PROXIMITY12', 'PIN_PROXIMITY13', 'PIN_PROXIMITY14',
                'PIN_PROXIMITY15', 'PIN_PROXIMITY16', 'PIN_PROXIMITY17', 'PIN_PROXIMITY18', 'PIN_PROXIMITY19',
                'PIN_PROXIMITY20', 'PIN_PROXIMITY21', 'PIN_PROXIMITY22', 'PIN_PROXIMITY23', 'PIN_PROXIMITY24',
                'PIN_PROXIMITY25', 'PIN_PROXIMITY26', 'PIN_PROXIMITY27','PIN_PROXIMITY28','PIN_PROXIMITY29']
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
            session_name = '%s-%s' %(task, modality)
            session_filename = '%s.txt' %session_name
            print("[Participant %d] Parsing %s..." %(participant_index, session_filename))
            isRestSession = True if 'REST' in session_filename else False
            participant_sequence = df_seq.loc[participant_index]
            # index of session_name in participant_sequence
            session_column_name = participant_sequence[participant_sequence == session_name].index[0]
            session_index = int(session_column_name[-1]) 
            if isRestSession:
                session_index = -session_index
            print("Session name: %s" %session_name)
            print("Session index: %s" %session_index)
            #exit()
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
                            'SESSION': session_index,
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
                df_trigger = pd.DataFrame(columns=df_log_columns)   
                system_file = open(os.path.join(data_directory, log_directory, participant_directory, session_filename), 'r')
                KEY_USER_ROTATION = "USER_ROTATION"
                KEY_SYSTEM_ROTATION = "SYSTEM_ROTATION"
                KEY_SC_START = "LANDSCAPE_ASCENDING"
                KEY_SC_END = "LANDSCAPE_ASCENDED"
                KEY_LEFT_HAND = "USER_LEFT_HAND"
                KEY_RIGHT_HAND = "USER_RIGHT_HAND"
                KEY_SC_POSITION = "SYSTEM_POSITION"
                KEY_SC_PROXIMITY = "SYSTEM_PROXIMITY"
                KEY_SC_ORIENTATION = "USER_PIN_ORIENTATION"
                KEY_START_MOLE = "MOLE_APPEARED"
                KEY_START_GAUGE = "GAUGE_APPEARED"
                KEY_END_MOLE = "MOLE_APPEARING"
                KEY_END_GAUGE = "GAUGE_APPEARING"
                lines = system_file.readlines()
                #print(lines[0])
                isTriggered = 0

                trial_index = -1
                is_in_trial = False
                is_first_user_rotation = False
                rotation_user = 0
                rotation_system = 0

                t_trial_start = None
                t_trial_end = None

                target_index = -1

                with alive_bar(len(lines)) as bar:
                    for line_index, line in enumerate(lines):
                        splitted_line =  line.strip().split('|')
                        if len(splitted_line) > 1:
                            t = pd.to_datetime(splitted_line[0], format='%Y-%m-%dT%H:%M:%S.%f')
                            parts = splitted_line[1].split(' ')

                            row = {
                                'PARTICIPANT':participant_index, 
                                'SESSION':session_index,
                                'TASK':task, 
                                'MODALITY': modality,
                                'DATE': t, 

                                'TARGET_POSITION': np.nan,
                                'TARGET_USER_ROTATION': np.nan,
                                'TARGET_ORIENTATION_X': np.nan,
                                'TARGET_ORIENTATION_Y': np.nan,
                                'TARGET_SYSTEM_ROTATION': np.nan,
                                'TRIAL_START': np.nan,
                                'TRIAL_END': np.nan,
                                'SC_START': np.nan,
                                'SC_END':  np.nan,
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

                                # pin proximity
                                'PIN_PROXIMITY0': np.nan,
                                'PIN_PROXIMITY1': np.nan,
                                'PIN_PROXIMITY2': np.nan,
                                'PIN_PROXIMITY3': np.nan,
                                'PIN_PROXIMITY4': np.nan,
                                'PIN_PROXIMITY5': np.nan,
                                'PIN_PROXIMITY6': np.nan,
                                'PIN_PROXIMITY7': np.nan,
                                'PIN_PROXIMITY8': np.nan,
                                'PIN_PROXIMITY9': np.nan,
                                'PIN_PROXIMITY10': np.nan,
                                'PIN_PROXIMITY11': np.nan,
                                'PIN_PROXIMITY12': np.nan,
                                'PIN_PROXIMITY13': np.nan,
                                'PIN_PROXIMITY14': np.nan,
                                'PIN_PROXIMITY15': np.nan,
                                'PIN_PROXIMITY16': np.nan,
                                'PIN_PROXIMITY17': np.nan,
                                'PIN_PROXIMITY18': np.nan,
                                'PIN_PROXIMITY19': np.nan,
                                'PIN_PROXIMITY20': np.nan,
                                'PIN_PROXIMITY21': np.nan,
                                'PIN_PROXIMITY22': np.nan,
                                'PIN_PROXIMITY23': np.nan,
                                'PIN_PROXIMITY24': np.nan,
                                'PIN_PROXIMITY25': np.nan,
                                'PIN_PROXIMITY26': np.nan,
                                'PIN_PROXIMITY27': np.nan,
                                'PIN_PROXIMITY28': np.nan,
                                'PIN_PROXIMITY29': np.nan
                                }
                            
                            # Check for duplicated index
                            duplicated_index = False
                            if not df_trigger[df_trigger['DATE'] == t].empty:
                                row = df_trigger[df_trigger['DATE'] == t].head(n=1)
                                duplicated_index = True
                            
                            modified_index = False

                            # Check for trial start
                            if parts[0] == KEY_START_GAUGE or parts[0] == KEY_START_MOLE :
                                #print("TRIAL START detected ! %s=%s or %s=%s" %(parts[0], KEY_START_GAUGE, parts[0], KEY_START_MOLE))
                                is_in_trial = True
                                is_first_user_rotation = True
                                rotation_user = 0
                                t_trial_start = t
                                trial_index +=1
                                target_index = np.nan
                                row['TRIAL_START'] = 1
                                modified_index = True

                            # Check for trial end
                            if (is_in_trial and (parts[0] == KEY_END_GAUGE or parts[0] == KEY_END_MOLE or line_index == len(lines) - 1)):
                                #print("TRIAL END detected ! %s=%s or %s=%s or %s=%s" %(parts[0], KEY_END_GAUGE, parts[0], KEY_END_MOLE, line_index, len(lines) - 1))
                                t_trial_end = t
                                # fill unknown user rotation at target
                                unknown_user_rotation = df_trigger[(df_trigger['TARGET_POSITION'] ==-1) & (df_trigger['DATE'] >= t_trial_start) & (df_trigger['DATE'] <= t_trial_end)]
                                #print("TRIAL END detected ! found %s unknow target position to fill with %s" %(unknown_user_rotation.shape[0], target_index))
                                df_trigger.loc[unknown_user_rotation.index.values, 'TARGET_POSITION'] = target_index
                                #print("TRIAL NB LINES: %s" %nb_line_trial)
                                
                                is_in_trial = False
                                row['TRIAL_END'] = 1
                                modified_index = True

                            # if not in trial then go to next line
                            # if is_in_trial : continue
                            if is_in_trial :
                                if parts[0] == KEY_SC_START:
                                    row['SC_START'] = 1
                                    modified_index = True

                                if parts[0] == KEY_SC_END:
                                    row['SC_END'] = 1
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
                                    
                                    #assert 1 != 1, row
                                    #print(row['RIGHT_HAND_POSITION_X'], row['RIGHT_HAND_POSITION_Y'], row['RIGHT_HAND_POSITION_Z'])
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
                                    #print(row['RIGHT_HAND_POSITION_X'], row['RIGHT_HAND_POSITION_Y'], row['RIGHT_HAND_POSITION_Z'])
                                    modified_index = True

                                if parts[0] == KEY_SC_ORIENTATION:
                                    row['TARGET_ORIENTATION_X'] = int(parts[1].replace(',','.'))
                                    row['TARGET_ORIENTATION_Y'] = int(parts[2].replace(',','.'))
                                    modified_index = True

                                if parts[0] == KEY_USER_ROTATION:
                                    # unknown rotation target
                                    row['TARGET_POSITION'] = -1
                                    
                                    if task == "USER":
                                        row['TARGET_USER_ROTATION'] = 1
                                    if task == "SYSTEM":
                                        if is_first_user_rotation:
                                            rotation_user = 105 #float(parts[1].replace(',','.'))
                                            is_first_user_rotation = False
                                        rotation_offset = int(parts[2].replace(',','.')) - int(parts[1].replace(',','.'))
                                        rotation_user = rotation_user - rotation_offset
                                        row['TARGET_USER_ROTATION'] = int(rotation_user)
                                        #print("TARGET_USER_POSITION: %d" %rotation_user)
                                    modified_index = True
                                
                                if parts[0] == KEY_SYSTEM_ROTATION:
                                    # Fill system rotation rows

                                    if task == "SYSTEM":
                                        new_rotation_system = int(float(parts[2].replace(',','.')))
                                        if new_rotation_system != rotation_system:
                                            rotation_system = new_rotation_system
                                            row['TARGET_SYSTEM_ROTATION'] = int(rotation_system)
                                            #print("TARGET_SYSTEM_POSITION: %d" %rotation_system)
                                    modified_index = True


                                if parts[0] == KEY_SC_PROXIMITY:
                                    # Fill pin proximity rows
                                    sc_proximities = pd.Series([float(p.replace(',','.')) for p in parts[1:]])
                                    for i in sc_proximities.index.values:
                                        row["PIN_PROXIMITY%s" %i] = sc_proximities.values[i]
                                    modified_index = True

                                if parts[0] == KEY_SC_POSITION:
                                    # Fill pin position rows
                                    sc_positions = pd.Series(parts[1:]).astype(int)
                                    for i in sc_positions.index.values:
                                        row["PIN_POSITION%s" %i] = sc_positions.values[i]
                                    modified_index = True
                                    # Checkfor target identity
                                    #if new_user_rotation:
                                    sc_gauge_positions = sc_positions[(sc_positions >= 18) & (sc_positions <= 22)] # add tolerance
                                    sc_empty_positions = sc_positions[sc_positions == 0]
                                    if len(sc_gauge_positions) == 1 and len(sc_empty_positions) == 29:
                                        next_target_index = sc_gauge_positions.index.values[0]
                                        if next_target_index != target_index:
                                            target_index = next_target_index
                                            #print("target identity: %s" %(target_index))
                                            # Backfill unknown gauge rotation
                                            # last_user_rotation = df_trigger[df_trigger['USER'] ==-1]
                                            # target_index = sc_gauge_positions.index.values[0]
                                            # df_trigger.loc[last_user_rotation.index.values, 'USER'] = target_index
                                            # #print(df_trigger[df_trigger['USER'] !=0].tail(n=1))
                                            # new_user_rotation = False    
                            if modified_index:
                                if duplicated_index:
                                    df_trigger.loc[row.index.values] = row.values
                                else:
                                    df_trigger = df_trigger.append(row, ignore_index=True)
                            bar()
                # Check that every unknown target rotation has been processed
                if(len(df_trigger[df_trigger['TARGET_POSITION'] == -1].index.values) != 0):
                    print("Unknown rotation detected! Removing occurencies...")
                    print(df_trigger[df_trigger['TARGET_POSITION'] == -1])
                    df_trigger[df_trigger['TARGET_POSITION'] == -1].to_csv("unknow_rotation_"+str(time.time()).replace('.', '_') + ".csv")
                    df_trigger[df_trigger['TARGET_POSITION'] == -1] = np.nan

                df_trigger.set_index('DATE', inplace=True,  verify_integrity=True)
                #Concatenate physio and log data and interpolate them.
                df_concat = pd.concat([df_physio, df_trigger])
                #df_concat.set_index('DATE', inplace=True,  verify_integrity=True)
                df_concat = df_concat.sort_index()
                # assert df_concat[df_concat['SC_START'] == 1].shape[0] == 9, "MISSING SHAPE-CHANGE START EVENT!"
                # assert df_concat[df_concat['SC_END'] == 1].shape[0] == 9, "MISSING SHAPE-CHANGE END EVENT!"
                # assert df_concat[df_concat['TRIAL_START'] == 1].shape[0] == 9, "MISSING TRIAL START EVENT!"
                # assert df_concat[df_concat['TRIAL_END'] == 1].shape[0] == 9, "MISSING TRIAL END EVENT!"

                duplicated_concat_indexes = df_concat.index.duplicated(keep='last')
                if duplicated_concat_indexes.sum() != 0:
                    print("Duplicates detected! Keeping last occurency...")
                    print(df_concat[duplicated_concat_indexes])
                    df_concat[duplicated_concat_indexes].to_csv("duplicated_index_"+str(time.time()).replace('.', '_') + ".csv")
                    df_concat = df_concat[~duplicated_concat_indexes]
                
                # assert df_concat to debug
                # print(df_concat[df_concat['TRIAL_START'] == 1])
                # print(df_concat[df_concat['TRIAL_END'] == 1])
                # print(df_concat[df_concat['SC_START'] == 1])
                # print(df_concat[df_concat['SC_END'] == 1])
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

                df_concat = df_concat.infer_objects()
                trial_start_indexes = df_concat[df_concat['TRIAL_START'] == 1].index
                trial_end_indexes = df_concat[df_concat['TRIAL_END'] == 1].index
                trial_i = 0
                for(trial_start_index, trial_end_index) in zip(trial_start_indexes, trial_end_indexes):
                    df_trial = df_concat.loc[trial_start_index:trial_end_index]
                    #print(df_trial)
                    #print(df_trial.dtypes)
                    nb_gsr = df_trial[~np.isnan(df_trial['GSR'])].shape[0]
                    nb_bvp = df_trial[~np.isnan(df_trial['BVP'])].shape[0]
                    nb_tmp = df_trial[~np.isnan(df_trial['TMP'])].shape[0]
                    if nb_gsr == 0 or nb_bvp == 0 or nb_tmp == 0:
                        warn("Lost physio data for trial %s (%s GRS found, %s BVP found, %s TMP found)" % (trial_i, nb_gsr, nb_bvp, nb_tmp))
                    trial_i += 1
                


                # for unique_duplicates_index in unique_duplicates_indexes:
                    
                #      print(df_duplicates_concat)
                #      df_concat.to_csv("debug_concat.csv")
                #      df_duplicates_concat.to_csv("debug_duplicates_concat.csv")
                #      sys.exit()
                #remove NaN and interpolate
                #df_concat[['SYSTEM', 'USER']] = df_concat[['SYSTEM', 'USER']].fillna(0)
                #df_concat.interpolate(inplace=True)
                df_final = df_final.append(df_concat, ignore_index=False)
    
# Save all data
df_final.to_csv(os.path.join(parse_directory, 'all.csv'))
