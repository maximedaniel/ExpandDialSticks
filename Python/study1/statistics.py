
import numpy as np
import pandas as pd
from scipy import stats
import time
import statsmodels.api as sm
import statsmodels.stats.descriptivestats as smdesc
import math
import os
import sys
import pingouin as pg


from plotter.StackedBarPlotter import StackedBarPlotter 
from plotter.ScatterPlotter import ScatterPlotter
from plotter.BoxPlotter import BoxPlotter 

dataFile = 'data.xlsx'
imgDir = 'img'

if not os.path.exists(imgDir):
    os.makedirs(imgDir)


class Statistics:

    @staticmethod
    def qualNominalPaired(sheetName, sheetDf):
        print("######################################## ",sheetName," ########################################")
        meltedSheetDf = sheetDf.melt(var_name='columns', value_name='index')
        contingencySheetTable = pd.crosstab(index=meltedSheetDf['index'], columns=meltedSheetDf['columns'])
        #if len(splittedSheetName) > 1:
        #     orderedColumns = splittedSheetName[1].split('>')
        #     contingencySheetTable = contingencySheetTable.reindex(orderedColumns)
        contingencySheetTable.loc['COUNT'] = contingencySheetTable.sum().values
        contingencySheetTable = contingencySheetTable.drop(index='COUNT')
        
        if len(sheetDf.columns) > 2:
            print(contingencySheetTable)
            chi2, pvalue, dof, ex = stats.chi2_contingency(contingencySheetTable.T)
            print( sheetDf.columns.str.cat(sep=' | '), " -> CHI² (statistic: ", chi2, ", p-value: ", pvalue,")")
        for i in range(len(sheetDf.columns.values)):
            for j in range(i+1, len(sheetDf.columns.values)):
                try:
                    ctab = contingencySheetTable[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
                    print(ctab)
                    chi2, pvalue, dof, ex = stats.chi2_contingency(ctab)
                    print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> CHI² (statistic: ", chi2, ", p-value: ", pvalue,")")
                except ValueError as ChiError:
                    print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> CHI² (",ChiError,")")
                    try:
                        ctab = contingencySheetTable[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
                        oddsratio, pvalue = stats.fisher_exact(ctab)
                        print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j], " -> Fisher (statistic: ", oddsratio, ", p-value: ", pvalue,")")
                    except ValueError as FisherError:
                        print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j], " -> Fisher (", FisherError,")")
    
    @staticmethod
    def qualNominalUnpaired(sheetName, sheetDf):
        print("######################################## ",sheetName," ########################################")
        meltedSheetDf = sheetDf.melt(var_name='columns', value_name='index')
        contingencySheetTable = pd.crosstab(index=meltedSheetDf['index'], columns=meltedSheetDf['columns'])
        #if len(splittedSheetName) > 1:
        #    orderedColumns = splittedSheetName[1].split('>')
        #    contingencySheetTable = contingencySheetTable.reindex(orderedColumns)
        contingencySheetTable.loc['COUNT'] = contingencySheetTable.sum().values
        
        if len(sheetDf.columns) > 2:
            print(contingencySheetTable)
            contingencySheetTable = contingencySheetTable.drop(index='COUNT')
            chi2, pvalue, dof, ex = stats.chi2_contingency(contingencySheetTable.T)
            print( sheetDf.columns.str.cat(sep=' | '), " -> CHI² (statistic: ", chi2, ", p-value: ", pvalue,")")
        
        for i in range(len(sheetDf.columns.values)):
            for j in range(i+1, len(sheetDf.columns.values)):
                try:
                    ctab = contingencySheetTable[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
                    print(ctab)
                    chi2, pvalue, dof, ex = stats.chi2_contingency(ctab)
                    print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> CHI² (statistic: ", chi2, ", p-value: ", pvalue,")")
                except ValueError as ChiError:
                    print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> CHI² (",ChiError,")")
                    try:
                        ctab = contingencySheetTable[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
                        oddsratio, pvalue = stats.fisher_exact(ctab)
                        print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j], " -> Fisher (statistic: ", oddsratio, ", p-value: ", pvalue,")")
                    except ValueError as FisherError:
                        print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j], " -> Fisher (", FisherError,")")
        StackedBarPlotter(
         filename =  imgDir + '/' + sheetName + '.png', 
         title = sheetName,
         sheetDf = contingencySheetTable)

    @staticmethod
    def qualOrdinalPaired(sheetName, sheetDf, sheetScale):
        print("######################################## ",sheetName," ########################################")
        comparisonMode = 1
        # aggregate
        allDf = pd.DataFrame()
        task_names = sheetDf['TASK'].unique()
        modality_names = sheetDf['MODALITY'].unique()
        for task_name in task_names:
            for modality_name in modality_names:
                modalityDf = sheetDf[(sheetDf['TASK'] == task_name) & (sheetDf['MODALITY'] == modality_name)]
                allDf[task_name+'-'+modality_name] = modalityDf.iloc[:, 2].values        
        print(smdesc.describe(allDf, stats=['nobs', 'missing', 'mean', 'ci', 'median', 'min', 'max']))
        meltedAllDf = allDf.melt(var_name='columns', value_name='index')
        contingencyAllTable = pd.crosstab(index=meltedAllDf['index'], columns=meltedAllDf['columns'])
        statDf = pd.DataFrame(columns=['COMPARISON', 'TEST', 'STATISTICS', 'P-VALUE', 'EFFECT SIZE'])
        #fill empty scale value
        for sheetStep in range(sheetScale):
            if not sheetStep in contingencyAllTable.index.values:
                #print("add %d" %sheetStep)
                contingencyAllTable.loc[sheetStep] = [0 for x in range(len(contingencyAllTable.columns.values))]
        contingencyAllTable.sort_index(inplace=True)

        # contingencyAllTable.loc['COUNT'] = contingencyAllTable.sum().values
        # contingencyAllTable = contingencySheetTable.drop(index='COUNT')
        
        if len(allDf.columns) > 2:
            friedmanDf = pd.DataFrame(columns=['PARTICIPANT', 'MODALITY', 'SCORE'])
            modality_names = allDf.columns.values
            for index, row in allDf.iterrows():
                for modality_name in modality_names:
                    friedmanDf = friedmanDf.append({'PARTICIPANT': index, 'MODALITY': modality_name, 'SCORE': float(row[modality_name])}, ignore_index=True)
            friedman_stats = pg.friedman(data=friedmanDf, dv='SCORE', within='MODALITY', subject='PARTICIPANT')
            source, wvalue, ddof1, qvalue, pvalue = friedman_stats.values[0]
            statDf = statDf.append(
                {'COMPARISON': 'ALL',
                'TEST': "Friedman",
                'STATISTICS':qvalue,
                'P-VALUE':pvalue,
                'EFFECT SIZE':wvalue
                }, ignore_index=True)
        # BETWEEN ALL (TO REMOVE?)
        
        if comparisonMode == 0 :
            modality_names = allDf.columns.values
            uncorrectedStatIndex = len(statDf.index)
            for i in range(len(modality_names)):
                for j in range(i+1, len(modality_names)):
                        stats_wilcoxon = pg.wilcoxon(allDf.loc[:, modality_names[i]], allDf.loc[:, modality_names[j]], correction=False, alternative='two-sided')
                        wvalue, alternative, pvalue, RBC, CLES = stats_wilcoxon.values[0]
                        statDf = statDf.append(
                            {
                                'COMPARISON': modality_names[i] + '|' + modality_names[j],
                                'TEST': "Wilcoxon",
                                'STATISTICS':wvalue,
                                'P-VALUE':pvalue,
                                'EFFECT SIZE': RBC
                            }, ignore_index=True)
            reject, statDf.loc[uncorrectedStatIndex::,'P-VALUE'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'P-VALUE'].values, alpha=0.05, method="holm")
        
        if comparisonMode == 1:
            # BETWEEN TASKS
            task_names = sheetDf['TASK'].unique()
            uncorrectedStatIndex = len(statDf.index)
            for i in range(len(task_names)):
                for j in range(i+1, len(task_names)):
                        headTaskValues = allDf.loc[:, allDf.columns.str.startswith(task_names[i])].values.reshape(-1)
                        tailTaskValues = allDf.loc[:, allDf.columns.str.startswith(task_names[j])].values.reshape(-1)
                        stats_wilcoxon = pg.wilcoxon(headTaskValues, tailTaskValues, correction=False, alternative='two-sided')
                        wvalue, alternative, pvalue, RBC, CLES = stats_wilcoxon.values[0]
                        statDf = statDf.append(
                            {
                                'COMPARISON': task_names[i] + '|' + task_names[j],
                                'TEST': "Wilcoxon",
                                'STATISTICS':wvalue,
                                'P-VALUE':pvalue,
                                'EFFECT SIZE': RBC
                            }, ignore_index=True)
            reject, statDf.loc[uncorrectedStatIndex::,'P-VALUE'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'P-VALUE'].values, alpha=0.05, method="holm")
            
            # MODALITIES WITHIN TASKS
            modality_names = sheetDf['MODALITY'].unique()
            for task_name in task_names:
                taskDf = allDf.loc[:, allDf.columns.str.startswith(task_name)]
                uncorrectedStatIndex = len(statDf.index)
                for i in range(len(modality_names)):
                    for j in range(i+1, len(modality_names)):
                        headModalityValues = taskDf.loc[:, taskDf.columns.str.endswith(modality_names[i])].values.reshape(-1)
                        tailModalityValues = taskDf.loc[:, taskDf.columns.str.endswith(modality_names[j])].values.reshape(-1)
                        stats_wilcoxon = pg.wilcoxon(headModalityValues, tailModalityValues, correction=False, alternative='two-sided')
                        wvalue, alternative, pvalue, RBC, CLES = stats_wilcoxon.values[0]
                        statDf = statDf.append(
                            {
                                'COMPARISON': task_name+'-'+modality_names[i] + '|' + task_name+'-'+ modality_names[j],
                                'TEST': "Wilcoxon",
                                'STATISTICS':wvalue,
                                'P-VALUE':pvalue,
                                'EFFECT SIZE': RBC
                            }, ignore_index=True)
                reject, statDf.loc[uncorrectedStatIndex::,'P-VALUE'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'P-VALUE'].values, alpha=0.05, method="holm")


            # BETWEEN MODALITIES
            modality_names = sheetDf['MODALITY'].unique()
            uncorrectedStatIndex = len(statDf.index)
            for i in range(len(modality_names)):
                for j in range(i+1, len(modality_names)):
                        headModalityValues = allDf.loc[:, allDf.columns.str.endswith(modality_names[i])].values.reshape(-1)
                        tailModalityValues = allDf.loc[:, allDf.columns.str.endswith(modality_names[j])].values.reshape(-1)
                        stats_wilcoxon = pg.wilcoxon(headModalityValues, tailModalityValues, correction=False, alternative='two-sided')
                        wvalue, alternative, pvalue, RBC, CLES = stats_wilcoxon.values[0]
                        statDf = statDf.append(
                            {
                                'COMPARISON': modality_names[i] + '|' + modality_names[j],
                                'TEST': "Wilcoxon",
                                'STATISTICS':wvalue,
                                'P-VALUE':pvalue,
                                'EFFECT SIZE': RBC
                            }, ignore_index=True)
            reject, statDf.loc[uncorrectedStatIndex::,'P-VALUE'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'P-VALUE'].values, alpha=0.05, method="holm")
            
            # TASKS WITHIN MODALITIES
            task_names = sheetDf['TASK'].unique()
            for modality_name in modality_names:
                modalityDf = allDf.loc[:, allDf.columns.str.endswith(modality_name)]
                uncorrectedStatIndex = len(statDf.index)
                for i in range(len(task_names)):
                    for j in range(i+1, len(task_names)):
                        headTaskValues = modalityDf.loc[:, modalityDf.columns.str.startswith(task_names[i])].values.reshape(-1)
                        tailTaskValues = modalityDf.loc[:, modalityDf.columns.str.startswith(task_names[j])].values.reshape(-1)
                        stats_wilcoxon = pg.wilcoxon(headTaskValues, tailTaskValues, correction=False, alternative='two-sided')
                        wvalue, alternative, pvalue, RBC, CLES = stats_wilcoxon.values[0]
                        statDf = statDf.append(
                            {
                                'COMPARISON': task_names[i] + '-' + modality_name + '|' + task_names[j] + '-' + modality_name,
                                'TEST': "Wilcoxon",
                                'STATISTICS':wvalue,
                                'P-VALUE':pvalue,
                                'EFFECT SIZE': RBC
                            }, ignore_index=True)
                reject, statDf.loc[uncorrectedStatIndex::,'P-VALUE'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'P-VALUE'].values, alpha=0.05, method="holm")
        print(statDf)
        #sys.exit()

        StackedBarPlotter(
         filename =  imgDir + '/' + sheetName + '.png', 
         title = sheetName,
         dataDf = allDf,
         histDf = contingencyAllTable,
         statDf = statDf)

    @staticmethod
    def qualOrdinalUnpaired(sheetName, sheetDf, sheetScale):
        print("######################################## ",sheetName," ########################################")
        meltedSheetDf = sheetDf.melt(var_name='columns', value_name='index')
        contingencySheetTable = pd.crosstab(index=meltedSheetDf['index'], columns=meltedSheetDf['columns'])
        
        #fill empty scale value
        for sheetStep in range(sheetScale):
            if not sheetStep in contingencySheetTable.index.values:
                print("add %d" %sheetStep)
                contingencySheetTable.loc[sheetStep] = [0 for x in range(len(contingencySheetTable.columns.values))]
        contingencySheetTable.sort_index(inplace=True)

        contingencySheetTable.loc['COUNT'] = contingencySheetTable.sum().values
        contingencySheetTable = contingencySheetTable.drop(index='COUNT')
        if len(sheetDf.columns) > 2:
            print(sheetDf)
            statistic, pvalue = stats.kruskal( *[content.values for label, content in sheetDf.iteritems()])
            print( sheetDf.columns.str.cat(sep=' | '), " -> Kruskal-Wallis (statistic:", statistic, " p-value: ", pvalue, ")")
        for i in range(len(sheetDf.columns.values)):
            for j in range(i+1, len(sheetDf.columns.values)):
                try:
                    ctab = contingencySheetTable[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
                    print(ctab)
                    ans = sm.stats.Table(ctab).test_ordinal_association()
                    print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> Cochran-Armitage (statistic: ", ans.statistic, ", p-value: ", ans.pvalue,")")
                except ValueError as CochranArmitageError:
                    print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> Cochran-Armitage (",CochranArmitageError,")")
        StackedBarPlotter(
         filename =  imgDir + '/' + sheetName + '.png', 
         title = sheetName,
         sheetDf = contingencySheetTable)
    
    @staticmethod
    def quantPaired(sheetName, sheetDf):
        print("######################################## ",sheetName," ########################################")
        if len(sheetDf.columns) > 2:
            print(sheetDf)
            statistic, pvalue = stats.friedmanchisquare( *[content.values for label, content in sheetDf.iteritems()])
            print( sheetDf.columns.str.cat(sep=' | '), " -> Friedman (statistic:", statistic, " p-value: ", pvalue, ")")

        for i in range(len(sheetDf.columns.values)):
            for j in range(i+1, len(sheetDf.columns.values)):
                try:
                    df = sheetDf[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
                    print(df)
                    statistic, pvalue = stats.ttest_rel(*[content.values for label, content in df.iteritems()])
                    print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> Student (statistic: ", statistic, ", p-value: ", pvalue,")")
                except ValueError as StudentError:
                    print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> Student (",StudentError,")")
        BoxPlotter(
         filename =  imgDir + '/' + sheetName + '.png', 
         title = sheetName,
         sheetDf = sheetDf)

    @staticmethod
    def quantUnpaired(sheetName, sheetDf):
        print("######################################## ",sheetName," ########################################")
        if len(sheetDf.columns) > 2:
            print(sheetDf)
            statistic, pvalue = stats.f_oneway(*[content.values for label, content in sheetDf.iteritems()])
            print( sheetDf.columns.str.cat(sep=' | '), " -> ANOVA (statistic:", statistic, " p-value: ", pvalue, ")")
        for i in range(len(sheetDf.columns.values)):
            for j in range(i+1, len(sheetDf.columns.values)):
                try:
                    df = sheetDf[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
                    print(df)
                    statistic, pvalue = stats.ttest_ind(*[content.values for label, content in df.iteritems()])
                    print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> Student (statistic: ", statistic, ", p-value: ", pvalue,")")
                except ValueError as StudentError:
                    print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> Student (",StudentError,")")
        BoxPlotter(
         filename =  imgDir + '/' + sheetName + '.png', 
         title = sheetName,
         sheetDf = sheetDf)
         
    @staticmethod
    def biQuantUnpaired(sheetName, sheetDf):
        print("######################################## ",sheetName," ########################################")
        try:
            print(sheetDf)
            coefficient, pvalue = stats.pearsonr(sheetDf.iloc[:,0], sheetDf.iloc[:,1])
            print(sheetDf.columns.values[0],'|', sheetDf.columns.values[1], " -> Pearson Correlation (coefficient: ", coefficient,", pvalue: ", pvalue, ")")
            try:
                slope, intercept, r_value, p_value, std_err = stats.linregress(sheetDf.iloc[:,0], sheetDf.iloc[:,1])
                print(sheetDf.columns.values[0],'|', sheetDf.columns.values[1], " -> Linear Regression (slope: ",slope, ", intercept: ", intercept, ", r_value: ", r_value, ", p_value: ", p_value, ", std_err: ", std_err, ")")
            except ValueError as LinearRegressError:
                print(sheetDf.columns.values[0],'|', sheetDf.columns.values[1], " -> Linear Regression (",LinearRegressError,")")
        except ValueError as PearsonError:
            print(sheetDf.columns.values[0],'|', sheetDf.columns.values[1], " -> Pearson Correlation (",PearsonError,")")
       
        ScatterPlotter(
         filename =  imgDir + '/' + sheetName + '.png', 
         title = sheetName,
         sheetDf = sheetDf,
         slope= slope,
         intercept= intercept)

         
# # Parse .xlsx file
# dictDf = pd.read_excel(dataFile, sheet_name=None)
# # Iterating key, value over Excel dictionary
# for sheetName, sheetDf in dictDf.items():
#     splittedSheetName = sheetName.split('|')
#     statType = splittedSheetName[1]
#     switcher = {
#         'QNP': Statistic.qualNominalPaired,
#         'QNU': Statistic.qualNominalUnpaired,
#         'QOP': Statistic.qualOrdinalPaired,
#         'QOU': Statistic.qualOrdinalUnpaired,
#         'QP': Statistic.quantPaired,
#         'QU': Statistic.quantUnpaired,
#         '2QU': Statistic.biQuantUnpaired,
#     }
#     statFunc = switcher.get(statType, lambda: "Bad statType")
#     statFunc(splittedSheetName[0], sheetDf)


