import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
import shutil
from zipfile import ZipFile
import sys
import re
from alive_progress import alive_bar
from utils import *
plotting = True
printing = True
saving = False

sequence_filename = 'sequence.xlsx'
data_directory = 'data'
archive_directory = 'archive'
log_directory = 'Logs'
physio_directory = 'Physios'
form_directory = 'Forms'
interview_directory = 'Interviews'
physio_columns = ['rest0', 'train0', 'session0', 'rest1', 'session1', 'rest2', 'train1', 'session2', 'rest3', 'session3']
log_columns = ['train0', 'session0', 'session1', 'train1', 'session2', 'session3']

df_seq = pd.read_excel(sequence_filename, index_col=0)

stampRegex = r'\d\d\d\d-\d\d-\d\d[T]\d\d[:]\d\d[:]\d\d(?:[.]\d\d\d\d\d\d)?[|]'
physioRegex = r'\d\d\d\d-\d\d-\d\d[T]\d\d[:]\d\d[:]\d\d(?:[.]\d\d\d\d\d\d)?[|](\bE4_Gsr\b|\bE4_Bvp\b|\bE4_Hr\b|\bE4_Ibi\b|\bE4_Temperature\b)\s(\d+|\d+[,]\d+)\s-?(\d+|\d+[,]\d+)\n'
physioChecker = re.compile(physioRegex)
stampChecker = re.compile(stampRegex)
print("Parsing data...")
# Unzip every participant archive
for (dirpath, dirnames, filenames) in os.walk(archive_directory):
    for filename in filenames:
        with ZipFile(os.path.join(dirpath, filename), 'r') as zipFile:
            participant_directory = os.path.join(dirpath.replace(archive_directory, data_directory), filename.replace('.zip', ''))
            print(os.path.basename(participant_directory))
            # remove dir if exists and unzip archive
            try:
                shutil.rmtree(participant_directory)
            except OSError as e:
                print("Error: %s : %s" % (participant_directory, e.strerror))
            zipFile.extractall(participant_directory)
            # Rename every file in Logs dir
            #print('Participant number %s' %participant_index)
            if log_directory in participant_directory:
                print("\t%s" %(log_directory))
                participant_index = int(filename.split('participant')[1].split('.')[0])
                i = 0
                log_list_dir = os.listdir( participant_directory )
                if len(log_list_dir) != 6: sys.exit("[ERROR] Missing Log files.")
                for file in log_list_dir:
                    log_column = log_columns[i]
                    sequence_tag = df_seq.iloc[participant_index][log_column] + '.txt'
                    file_path =  os.path.join(participant_directory, file)
                    new_file_path = os.path.join(participant_directory, sequence_tag) 
                    i += 1
                    os.rename(file_path, new_file_path)
                    print("\t\t%s -> %s" %(file, sequence_tag))
            # Rename every file in Physios dir
            if physio_directory in participant_directory:
                print("\t%s" %(physio_directory))
                participant_index = int(filename.split('participant')[1].split('.')[0])
                i = 0
                physio_list_dir = os.listdir( participant_directory )
                if len(physio_list_dir) != 10: sys.exit("[ERROR] Missing Physio files.")
                for file in physio_list_dir:
                    physio_column = physio_columns[i]
                    sequence_tag = df_seq.iloc[participant_index][physio_column] + '.txt'
                    file_path =  os.path.join(participant_directory, file)
                    new_file_path = os.path.join(participant_directory, sequence_tag) 
                    i += 1
                    os.rename(file_path, new_file_path)
                    print("\t\t%s -> %s" %(file, sequence_tag))
                    clean_physio_lines = ""
                    with open(new_file_path, 'r') as physio_file:
                        lines = physio_file.readlines()
                        for line in lines:
                            if line == "\n":
                                continue
                            matched = physioChecker.match(line)
                            if matched is None:
                                notOk("%s" %(line))
                                #repair the chaine
                                stampMatched = stampChecker.match(line)
                                stampSplitted = stampChecker.split(line)
                                line = stampMatched.group(0) + ''.join(stampSplitted)
                                ok("%s" %(line))
                            clean_physio_lines+=line
                    with open(new_file_path, 'w') as physio_file:
                        physio_file.seek(0)
                        physio_file.truncate()
                        physio_file.write(clean_physio_lines)
                     # TODO CHECK FOR DATA INTEGRITY => no 2 '|' in a single row
            # Do nothing for every file in Forms dir

