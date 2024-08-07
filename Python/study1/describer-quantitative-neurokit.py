import math
import pandas as pd
import neurokit2 as nk
import numpy as np
import matplotlib.pyplot as plt
import os
from plotter.HeatMapPlotter import HeatMapPlotter 
from plotter.SpacePlotter import SpacePlotter 
from plotter.SignalPlotter import SignalPlotter 
from utils import *
import warnings
warnings.simplefilter(action='ignore', category=FutureWarning)
imgDir = 'img'
BVP_HZ = 64
EPOCH_START = -2
EPOCH_DURATION = 10
PRINTING = False
PLOTTING = False
SAVING = False
NB_TRIALS = 9
NB_ROWS = 5
NB_COLUMNS = 6
TASK_NAMES = ['SYSTEM', 'USER']
MODALITY_NAMES = ['SMS-REST', 'SMS', 'SSM-REST', 'SSM']
INPUT_COLUMN_NAMES = ['DATE', 'PARTICIPANT', 'SESSION', 'TASK', 'MODALITY', 'GSR', 'BVP', 'TMP',
       'TARGET_POSITION', 'TARGET_ORIENTATION_X', 'TARGET_ORIENTATION_Y',
       'TARGET_USER_ROTATION', 'TARGET_SYSTEM_ROTATION', 'TRIAL_START',
       'TRIAL_END', 'SC_START', 'SC_END', 
        # left hand and arm transform
        'LEFT_HAND_POSITION_X', 'LEFT_HAND_POSITION_Y', 'LEFT_HAND_POSITION_Z', 'LEFT_HAND_RADIUS',
        'LEFT_ARM_POSITION_X','LEFT_ARM_POSITION_Y', 'LEFT_ARM_POSITION_Z',
        'LEFT_ARM_QUAT_A','LEFT_ARM_QUAT_B', 'LEFT_ARM_QUAT_C', 'LEFT_ARM_QUAT_D',
        'LEFT_ARM_RADIUS', 'LEFT_ARM_HEIGHT',
        # LEFT hand and arm transform
        'RIGHT_HAND_POSITION_X', 'RIGHT_HAND_POSITION_Y', 'RIGHT_HAND_POSITION_Z', 'RIGHT_HAND_RADIUS',
        'RIGHT_ARM_POSITION_X','RIGHT_ARM_POSITION_Y', 'RIGHT_ARM_POSITION_Z',
        'RIGHT_ARM_QUAT_A','RIGHT_ARM_QUAT_B', 'RIGHT_ARM_QUAT_C', 'RIGHT_ARM_QUAT_D',
        'RIGHT_ARM_RADIUS', 'RIGHT_ARM_HEIGHT',
        'PIN_POSITION0', 'PIN_POSITION1', 'PIN_POSITION2', 'PIN_POSITION3', 'PIN_POSITION4',
       'PIN_POSITION5', 'PIN_POSITION6', 'PIN_POSITION7', 'PIN_POSITION8',
       'PIN_POSITION9', 'PIN_POSITION10', 'PIN_POSITION11', 'PIN_POSITION12',
       'PIN_POSITION13', 'PIN_POSITION14', 'PIN_POSITION15', 'PIN_POSITION16',
       'PIN_POSITION17', 'PIN_POSITION18', 'PIN_POSITION19', 'PIN_POSITION20',
       'PIN_POSITION21', 'PIN_POSITION22', 'PIN_POSITION23', 'PIN_POSITION24',
       'PIN_POSITION25', 'PIN_POSITION26', 'PIN_POSITION27', 'PIN_POSITION28',
       'PIN_POSITION29', 'PIN_PROXIMITY0', 'PIN_PROXIMITY1', 'PIN_PROXIMITY2',
       'PIN_PROXIMITY3', 'PIN_PROXIMITY4', 'PIN_PROXIMITY5', 'PIN_PROXIMITY6',
       'PIN_PROXIMITY7', 'PIN_PROXIMITY8', 'PIN_PROXIMITY9', 'PIN_PROXIMITY10',
       'PIN_PROXIMITY11', 'PIN_PROXIMITY12', 'PIN_PROXIMITY13',
       'PIN_PROXIMITY14', 'PIN_PROXIMITY15', 'PIN_PROXIMITY16',
       'PIN_PROXIMITY17', 'PIN_PROXIMITY18', 'PIN_PROXIMITY19',
       'PIN_PROXIMITY20', 'PIN_PROXIMITY21', 'PIN_PROXIMITY22',
       'PIN_PROXIMITY23', 'PIN_PROXIMITY24', 'PIN_PROXIMITY25',
       'PIN_PROXIMITY26', 'PIN_PROXIMITY27', 'PIN_PROXIMITY28',
       'PIN_PROXIMITY29']

OUTPUT_COLUMN_NAMES = [ 
    'Date', 'Participant', 'Task', 'Modality', 'Trial' , 
    'Target_X', 'Target_Y',
    'SC_Count_Mean', 'SC_Count_SD', 'SC_Count_Max', 'SC_Count_Min',
    'SC_Amplitude_Mean', 'SC_Amplitude_SD', 'SC_Amplitude_Max', 'SC_Amplitude_Min',
    # Epochs
    'Label',
    'Event_Onset',
    # eda event-related features
    'EDA_SCR', # indication of whether Skin Conductance Response (SCR) occurs following the event (1 if an SCR onset is present and 0 if absent) and if so, its corresponding peak amplitude, time of peak, rise and recovery time. If there is no occurrence of SCR, nans are displayed for the below features.
    'EDA_Peak_Amplitude', #  the maximum amplitude of the phasic component of the signal.
    'SCR_Peak_Amplitude', #  the peak amplitude of the first SCR in each epoch.
    'SCR_Peak_Amplitude_Time', #  the timepoint of each first SCR peak amplitude.
    'SCR_RiseTime', #  the risetime of each first SCR i.e., the time it takes for SCR to reach peak amplitude from onset.
    'SCR_RecoveryTime', #  the half-recovery time of each first SCR i.e., the time it takes for SCR to decrease to half amplitude.

     # ppg event-related features
     'PPG_Rate_Baseline', # the baseline heart rate (at stimulus onset).
     'PPG_Rate_Max', #the maximum heart rate after stimulus onset.
     'PPG_Rate_Min', #the minimum heart rate after stimulus onset.
     'PPG_Rate_Mean', #he mean heart rate after stimulus onset.
     'PPG_Rate_SD', # the standard deviation of the heart rate after stimulus onset.
     'PPG_Rate_Max_Time', #the time at which maximum heart rate occurs.
     'PPG_Rate_Min_Time', # the time at which minimum heart rate occurs.
     # We also include the following experimental features related to the parameters of a quadratic model:
     'PPG_Rate_Trend_Linear', #The parameter corresponding to the linear trend.
     'PPG_Rate_Trend_Quadratic', # The parameter corresponding to the curvature.
     'PPG_Rate_Trend_R2', #the quality of the quadratic model. If too low, the parameters might not be reliable or meaningful.
]
parse_directory = "parse"
quantitative_file = 'all.csv'
df_quantitative = pd.read_csv(os.path.join(parse_directory, quantitative_file))
# for dat in df_quantitative['DATE']:
#     print(dat)
#     print(pd.to_datetime(dat, format='%Y-%m-%d %H:%M:%S.%f'))
df_quantitative['DATE'] = pd.to_datetime(df_quantitative['DATE'])
df_quantitative.set_index('DATE', inplace=True)
if df_quantitative.index.duplicated(keep='first').sum(): print("{ERROR] duplicated indexes found!")
df_output = pd.DataFrame(columns=OUTPUT_COLUMN_NAMES)
# PROCESS EACH PARTICIPANT
participants = df_quantitative['PARTICIPANT'].unique()
for participant in range (len(participants)):
    df_participant = df_quantitative[df_quantitative['PARTICIPANT'] == participant]
    # PROCESS EACH TASK
    for task in TASK_NAMES:
        # PROCESS EACH MODALITY
        for modality in MODALITY_NAMES:
            info("[Participant %s] Analyzing %s-%s..." %(participant,task,modality))
            isRestSession = True if 'REST' in modality else False
            # PROCESS EACH SESSION
            df_session =  df_participant[(df_participant['TASK'] == task) & (df_participant['MODALITY'] == modality)]
            sc_size_conditions = []
            # PROCESS EACH TRIAL
            trial_start_indexes = df_session[df_session['TRIAL_START'] == 1].index
            trial_end_indexes = df_session[df_session['TRIAL_END'] == 1].index
            trial_index = 0
            for(trial_start_index, trial_end_index) in zip(trial_start_indexes, trial_end_indexes):
                df_trial = df_session.loc[trial_start_index:trial_end_index]
                # PROCESS EACH SHAPE-CHANGE
                sc_start_indexes = df_trial[df_trial['SC_START'] == 1].index
                sc_end_indexes = df_trial[df_trial['SC_END'] == 1].index
                for(sc_start_index, sc_end_index) in zip(sc_start_indexes, sc_end_indexes):
                    
                    df_sc = df_trial.loc[sc_start_index:sc_end_index]
                    row = {
                        'Date':df_sc.index[0],
                        'Participant':participant, 
                        'Task':task, 
                        'Modality': modality,
                        'Trial' : trial_index,

                        'Target_X': np.nan,
                        'Target_Y': np.nan,

                        'SC_Count_Mean': np.nan,
                        'SC_Count_SD': np.nan,
                        'SC_Count_Max': np.nan,
                        'SC_Count_Min': np.nan,

                        'SC_Amplitude_Mean': np.nan,
                        'SC_Amplitude_SD': np.nan,
                        'SC_Amplitude_Max': np.nan,
                        'SC_Amplitude_Min': np.nan,
                        # eda event-related features
                        'EDA_SCR': np.nan, # indication of whether Skin Conductance Response (SCR) occurs following the event (1 if an SCR onset is present and 0 if absent) and if so, its corresponding peak amplitude, time of peak, rise and recovery time. If there is no occurrence of SCR, nans are displayed for the below features.
                        'EDA_Peak_Amplitude': np.nan, #  the maximum amplitude of the phasic component of the signal.
                        'SCR_Peak_Amplitude': np.nan, #  the peak amplitude of the first SCR in each epoch.
                        'SCR_Peak_Amplitude_Time': np.nan, #  the timepoint of each first SCR peak amplitude.
                        'SCR_RiseTime': np.nan, #  the risetime of each first SCR i.e., the time it takes for SCR to reach peak amplitude from onset.
                        'SCR_RecoveryTime': np.nan, #  the half-recovery time of each first SCR i.e., the time it takes for SCR to decrease to half amplitude.

                        # ppg event-related features
                        'PPG_Rate_Baseline': np.nan, # the baseline heart rate (at stimulus onset).
                        'PPG_Rate_Max': np.nan, #the maximum heart rate after stimulus onset.
                        'PPG_Rate_Min': np.nan, #the minimum heart rate after stimulus onset.
                        'PPG_Rate_Mean': np.nan, #he mean heart rate after stimulus onset.
                        'PPG_Rate_SD': np.nan, # the standard deviation of the heart rate after stimulus onset.
                        'PPG_Rate_Max_Time': np.nan, #the time at which maximum heart rate occurs.
                        'PPG_Rate_Min_Time': np.nan, # the time at which minimum heart rate occurs.
                        # We also include the following experimental features related to the parameters of a quadratic model:
                        'PPG_Rate_Trend_Linear': np.nan, #The parameter corresponding to the linear trend.
                        'PPG_Rate_Trend_Quadratic': np.nan, # The parameter corresponding to the curvature.
                        'PPG_Rate_Trend_R2': np.nan, #the quality of the quadratic model. If too low, the parameters might not be reliable or meaningful.
                                        }
                     # PROCESS RESTING
                    if isRestSession: # PROCESS INTERVAL RELATED
                        # PROCESS GSR
                        df_gsr = df_trial['GSR'].dropna()
                        # CHECK FREQUENCE INTEGRITY
                        freq_gsr = pd.infer_freq(df_gsr.index)
                        # RESAMPLE IF FREQ IS NONE
                        if freq_gsr is None:
                            df_gsr.index = pd.date_range(start=df_gsr.index[0], end=df_gsr.index[-1], periods=df_gsr.shape[0])
                            freq_gsr = pd.infer_freq(df_gsr.index)
                        # DOUBLE GSR FREQUENCY
                        for i in range(len(df_gsr.index.values) - 1):
                            curr_index_value = df_gsr.index.values[i]
                            curr_column_value = df_gsr[curr_index_value]
                            next_index_value = df_gsr.index.values[i+1]
                            next_column_value = df_gsr[next_index_value]
                            timedelta_index_value = next_index_value-curr_index_value
                            insert_index_value = curr_index_value + timedelta_index_value/2.0
                            insert_column_value = curr_column_value + (next_column_value-curr_column_value)/2.0
                            df_gsr[insert_index_value] = insert_column_value
                        df_gsr.sort_index(inplace=True)
                        freq_gsr = pd.infer_freq(df_gsr.index)
                        freq_delta_gsr = pd.Timedelta(freq_gsr)
                        hz_gsr = 1/freq_delta_gsr.total_seconds()
                        # Process EDA
                        eda_signals, eda_info = nk.bio_process(eda=df_gsr, sampling_rate=hz_gsr)
                        # analyze event-related features of BVP signals
                        # eda_analysis = nk.bio_analyze(eda_signals, method="interval-related")
                        
                        # Feature SCR (Skin Conductance Response)
                        eda_response_desc = eda_signals.loc[eda_signals['SCR_Amplitude'] > 0 ,["SCR_Amplitude"]].describe()
                        eda_response_max = eda_response_desc.loc['max'].values[0]
                        row['SCR_Peak_Amplitude'] = eda_response_max

                        # PROCESS BVP
                        df_bvp = df_trial['BVP'].dropna()
                        # Process ppg signal
                        ppg_signals, ppg_info = nk.bio_process(ppg=df_bvp, sampling_rate=BVP_HZ)
                        
                        # analyze event-related features of BVP signals
                        # ppg_analysis = nk.bio_analyze(ppg_signals, method="interval-related")

                        # describe BVP rate results
                        df_bvp_rate_desc = ppg_signals.loc[:, 'PPG_Rate'].describe()
                        bvp_rate_mean = df_bvp_rate_desc['mean']
                        row['PPG_Rate_Baseline'] = bvp_rate_mean
                        bvp_rate_std = df_bvp_rate_desc['std']
                        row['PPG_Rate_SD'] = bvp_rate_std
                        bvp_rate_max = df_bvp_rate_desc['max']
                        row['PPG_Rate_Max'] = bvp_rate_max
                        bvp_rate_min = df_bvp_rate_desc['min']
                        row['PPG_Rate_Min'] = bvp_rate_min
                    else: 
                        # PROCESS INTERVAL RELATED
                        # COMPUTE USEFUL VALUES
                        df_sc_trigger = df_trial.loc[df_trial['SC_START'] == 1, 'SC_START']
                        
                        sc_duration = (df_sc.tail(1).index - df_sc.head(1).index). total_seconds()
                        sc_duration = math.ceil(sc_duration[0]) * 2.0
                        # 6 SECONDS WINDOWS
                        # sc_duration = 6 #s for max window
                        # COMPUTE USER DEPTH INTO SHAPE-CHANGE
                        target_pin = -1
                        try:
                            target_pin = df_trial.loc[~np.isnan(df_trial['TARGET_POSITION']), 'TARGET_POSITION'].values[0].astype(int)
                        except:
                            warn("[Participant %s] Invalid target position at trial %s" %(participant, trial_index))
                        row['Target_Y'] = (NB_ROWS + 1) - (math.floor(target_pin/NB_COLUMNS) + 1)
                        row['Target_X'] = (NB_COLUMNS + 1) - (math.floor(target_pin%NB_COLUMNS) + 1)
                        #print("USER_DEPTH: %s" % user_depth)
                        # COMPUTE AMOUNT OF SHAPE-CHANGE
                        #print(target_pin)
                        pin_position_columns = [column_name for column_name in INPUT_COLUMN_NAMES if 'PIN_POSITION' in column_name and 'PIN_POSITION%s'%target_pin not in column_name]
                        df_pins = df_sc.loc[~np.isnan(df_sc['PIN_POSITION29']), pin_position_columns]
                        #print(df_pins)
                        df_sc_count = df_pins.diff(axis=0).fillna(0).astype(int)
                        df_sc_count[df_sc_count > 0] = 1
                        sc_count = df_sc_count.sum(axis = 1)
                        sc_count_describe = sc_count.describe()
                        row['SC_Count_Mean'] = sc_count_describe['mean']
                        row['SC_Count_SD'] = sc_count_describe['std']
                        row['SC_Count_Max'] = sc_count_describe['max']
                        row['SC_Count_Min'] = sc_count_describe['min']
                        #sc_count = df_pins_diff.where(df_pins_diff > 0, 1).count(axis = 1)
                        #print(sc_count)
                        sc_amplitude = df_pins.diff(axis=0).fillna(0).astype(int).sum(axis = 1)
                        sc_amplitude_describe = sc_amplitude.describe()
                        row['SC_Amplitude_Mean'] = sc_amplitude_describe['mean']
                        row['SC_Amplitude_SD'] = sc_amplitude_describe['std']
                        row['SC_Amplitude_Max'] = sc_amplitude_describe['max']
                        row['SC_Amplitude_Min'] = sc_amplitude_describe['min']
                        
                        
                        df_sc_signals = pd.DataFrame(
                            columns=['DATE', 'SC_COUNT', 'SC_AMPLITUDE'],
                            data={'DATE': sc_count.index.values, 'SC_COUNT': sc_count, 'SC_AMPLITUDE': sc_amplitude}
                            )
                        df_sc_signals.set_index('DATE', inplace=True,  verify_integrity=True)
                        df_sc_signals.index = (df_sc_signals.index - df_sc_trigger.index[0]).total_seconds()
                        sc_index_resampled = np.sort(np.concatenate((df_sc_signals.index.values, np.arange(EPOCH_START, sc_duration, 1/64))))
                        df_sc_signals = df_sc_signals.reindex(sc_index_resampled, fill_value=np.nan).interpolate(method='linear').fillna(0)

                        # PROCESS GSR and 
                        df_gsr = df_trial['GSR'].dropna()
                        nb_gsr = df_gsr.shape[0]
                        no_gsr = False
                        if nb_gsr > 0:
                            # CHECK FREQUENCE INTEGRITY
                            freq_gsr = pd.infer_freq(df_gsr.index)
                            # RESAMPLE IF FREQ IS NONE
                            if freq_gsr is None:
                                df_gsr.index = pd.date_range(start=df_gsr.index[0], end=df_gsr.index[-1], periods=df_gsr.shape[0])
                                freq_gsr = pd.infer_freq(df_gsr.index)
                            # double GSR signal
                            for i in range(len(df_gsr.index.values) - 1):
                                curr_index_value = df_gsr.index.values[i]
                                curr_column_value = df_gsr[curr_index_value]
                                next_index_value = df_gsr.index.values[i+1]
                                next_column_value = df_gsr[next_index_value]
                                timedelta_index_value = next_index_value-curr_index_value
                                insert_index_value = curr_index_value + timedelta_index_value/2.0
                                insert_column_value = curr_column_value + (next_column_value-curr_column_value)/2.0
                                df_gsr[insert_index_value] = insert_column_value
                            df_gsr.sort_index(inplace=True)
                            freq_gsr = pd.infer_freq(df_gsr.index)
                            #print("new freq: %s, gsr shape: %s" %(freq_gsr,df_gsr.shape))
                            freq_delta_gsr = pd.Timedelta(freq_gsr)
                            hz_gsr = 1/freq_delta_gsr.total_seconds()
                            
                            # extract events
                            df_event = pd.Series(index=df_gsr.index, data=0)
                            for event_raw_index in df_sc_trigger.index.values:
                                nearest_gsr_index = df_gsr.index.get_loc(event_raw_index, method="nearest")
                                df_event[nearest_gsr_index] = df_sc_trigger[event_raw_index]
                            # find events
                            eda_events = nk.events_find(df_event, threshold=0, threshold_keep='above')
                            # process eda signals
                            eda_signals, eda_info = nk.bio_process(eda=df_gsr, sampling_rate=hz_gsr)
                            # create epochs
                            eda_epochs_end = sc_duration if (sc_duration-EPOCH_START) <= EPOCH_DURATION else (EPOCH_START + EPOCH_DURATION)
                            eda_epochs = nk.epochs_create(eda_signals, eda_events, sampling_rate=hz_gsr, epochs_start=EPOCH_START, epochs_end=eda_epochs_end)
                            # analyze event-related features of EDA signals
                            eda_analysis = nk.bio_analyze(eda_epochs, method="event-related")
                            for columnName in eda_analysis.columns.values:
                                row[columnName] = eda_analysis[columnName].values[0]
                        else :
                            no_gsr = True
                        # COMPUTE BVP
                        df_bvp = df_trial['BVP'].dropna()
                        nb_bvp = df_bvp.shape[0]
                        no_bvp = False
                        if nb_bvp > 0:
                            #print("old freq: %s, bvp shape: %s" %(freq_bvp,df_bvp.shape))
                            # extract events
                            df_event = pd.Series(index=df_bvp.index, data=0)
                            for event_raw_index in df_sc_trigger.index.values:
                                nearest_bvp_index = df_bvp.index.get_loc(event_raw_index, method="nearest")
                                df_event[nearest_bvp_index] = df_sc_trigger[event_raw_index]
                            # find events
                            bvp_events = nk.events_find(df_event, threshold=0, threshold_keep='above')
                            # process signals
                            ppg_signals, ppg_info = nk.bio_process(ppg=df_bvp, sampling_rate=BVP_HZ)
                            # create epochs
                            ppg_epochs_end = sc_duration if (sc_duration-EPOCH_START) <= EPOCH_DURATION else (EPOCH_START + EPOCH_DURATION)
                            ppg_epochs = nk.epochs_create(ppg_signals, bvp_events, sampling_rate=BVP_HZ, epochs_start=EPOCH_START, epochs_end=ppg_epochs_end)
                            ppg_epoch = ppg_epochs['1']
                            # analyze event-related features of EDA signals
                            ppg_analysis = nk.bio_analyze(ppg_epochs, method="event-related")
                            for columnName in ppg_analysis.columns.values:
                                row[columnName] = ppg_analysis[columnName].values[0]
                        else :
                            no_bvp = True
                        if no_gsr or no_bvp:
                            warn("[Participant %s] No GSR/BVP data to process for trial %s" %(participant, trial_index))
                            
                        # PRINTING
                        if PRINTING:
                            for col in row:
                                print("%s: %s" %(col, row[col]))
                        # SignalPlotter(
                        #     filename = imgDir + '/P' + str(participant) + '_' +  task + '_' + modality + '_T' + str(trial_index) + '.png',
                        #     title = 'TEST', 
                        #     df_eda_signals = eda_signals, 
                        #     df_bvp_signals = ppg_signals, 
                        #     df_sc_signals = df_sc_signals, 
                        #     plotting =  PLOTTING, 
                        #     saving = SAVING)
                        # # Process it
                        # signals, info = nk.ppg_process(df_bvp, sampling_rate=64)
                        # # Visualize the processing
                        # nk.ppg_plot(signals, sampling_rate=64)
                        # plt.show()
                    df_output = df_output.append(row, ignore_index=True)
                trial_index +=1
            if not isRestSession and trial_index < NB_TRIALS:
                warn("[Participant %s] Missing %s/%s trials" %(participant, (NB_TRIALS -  trial_index + 1), NB_TRIALS))

df_output.set_index('Date', inplace=True)
df_output.to_csv(os.path.join(parse_directory, 'quantitative-description-neurokit.csv'))
