import pandas as pd
import neurokit2 as nk
import numpy as np
import matplotlib.pyplot as plt
import os
import statistics

parse_directory = "parse"
quantitative_file = 'all.csv'
df_quantitative = pd.read_csv(os.path.join(parse_directory, quantitative_file))
df_quantitative['DATE'] = pd.to_datetime(df_quantitative['DATE'])
df_quantitative.set_index('DATE', inplace=True)
if df_quantitative.index.duplicated(keep='first').sum(): print("{ERROR] duplicated indexes found!")
# Generate dataframe per task/modality
# Analysis USER SMS
participants = df_quantitative['PARTICIPANT'].unique()
for participant in participants:
    df_participant = df_quantitative[df_quantitative['PARTICIPANT'] == participant]
    df_participant_plot = df_participant.copy()
    df_participant_plot[['SYSTEM', 'USER']] = df_participant_plot[['SYSTEM', 'USER']].fillna(0)
    df_participant_plot = df_participant_plot.interpolate()
    df_participant_plot.plot(subplots=True, style="-")
    plt.show()
    exit()
    for task_index in ['USER', 'SYSTEM']:
        for modality_index in ['SMS', 'SSM']:
            print("[Participant %s] Analyzing %s-%s..." %(int(participant), task_index,modality_index))

            df_session =  df_participant[(df_participant['TASK'] == task_index) & (df_participant['MODALITY'] == modality_index)]

            # Process the signal
            df_gsr = df_session['GSR'].dropna()
            freq_gsr = pd.infer_freq(df_gsr.index)
            print("old freq: %s, gsr shape: %s" %(freq_gsr,df_gsr.shape))

            # DOUBLE SIGNAL FREQUENCY
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
            print("new freq: %s, gsr shape: %s" %(freq_gsr,df_gsr.shape))
            freq_delta_gsr = pd.Timedelta(freq_gsr)
            hz_gsr = 1/freq_delta_gsr.total_seconds()

            # EVENTS NON-NULL EXTRACTION
            df_event_raw = df_session['SYSTEM'].fillna(0)
            df_event_raw = df_event_raw[df_event_raw != 0]
            df_event = pd.Series(index=df_gsr.index, data=0)

            for event_raw_index in df_event_raw.index.values:
                nearest_gsr_index = df_gsr.index.get_loc(event_raw_index, method="nearest")
                df_event[nearest_gsr_index] = df_event_raw[event_raw_index]
            
            # find events
            events = nk.events_find(df_event, threshold= 0, threshold_keep='above')
            # process signals
            df, info = nk.bio_process(eda=df_gsr, sampling_rate=hz_gsr)
            # create epochs
            epochs = nk.epochs_create(df, events, sampling_rate=hz_gsr, epochs_start=-1, epochs_end=6)
            #print(events)
            plot = nk.events_plot(events, df_gsr.values)
            plt.show()
            
            df_results = {}

            for epoch_index in epochs:
                df_results[epoch_index] = {}  # then Initialize an empty dict inside of it with the iterative
                # Save a temp var with dictionary called <epoch_index> in epochs-dictionary
                epoch = epochs[epoch_index]
                #print(epoch.columns.values)

                # VISUALIZE EPOCH
                sub_epoch = epoch[['EDA_Phasic', 'EDA_Tonic', 'SCR_Amplitude']]  # Select relevant columns"
                title = epoch_index # get title from condition list",
                #nk.standardize(sub_epoch).plot(title=title, legend=True)  # Plot scaled signals"
                
                # We want its features:
                
                # Feature 2 EDA - SCR
                scr_max = epoch["SCR_Amplitude"].loc[0:600].max()  # Maximum SCR peak
                # If no SCR, consider the magnitude, i.e.  that the value is 0
                if np.isnan(scr_max):
                    scr_max = 0
                # Store SCR in df
                df_results[epoch_index]["SCR_Magnitude"] = scr_max

            df_results = pd.DataFrame.from_dict(df_results, orient="index")  # Convert to a dataframe
            df_results["Condition"] = range(len(epochs))  # Add the conditions
            #print(df_results)
            print(df_results.mean())

            # df, info = nk.bio_process(eda=df_gsr.values.tolist(), sampling_rate=hz_gsr)
            # df.plot(subplots=True)
            #plt.show()