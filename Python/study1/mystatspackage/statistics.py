
from turtle import Turtle
import numpy as np
import pandas as pd
from scipy import stats
import time
import statsmodels.api as sm
import statsmodels.stats.descriptivestats as smdesc
import matplotlib.pyplot as plt
import matplotlib.table as tab
import matplotlib.transforms as trans
import matplotlib.ticker as mticker
from matplotlib.text import OffsetFrom
from matplotlib.font_manager import FontProperties
import math
import os
import sys
import pingouin as pg
import seaborn as sns


from . import StackedBarPlotter
from . import ScatterPlotter
from .  import BoxPlotter

# if not os.path.exists(imgDir):
#     os.makedirs(imgDir)


class Statistics:
    @staticmethod
    def areRelatedFactors(factor1, factor2) -> bool:
        related = False
        subfactors1 = factor1.split('-')
        subfactors2 = factor2.split('-')
        if len(subfactors1) == 1 or len(subfactors2) == 1:
            related = True
        else :
            for subfactor1 in subfactors1:
                if subfactor1 in subfactors2:
                    related = True
        return related

    @staticmethod
    def describePlus(sheetDf):
        description = sheetDf.describe()
        # compute median, confidence interval
        medians = []
        modes = []
        freqs = []
        low_cis = []
        high_cis = []
        for columnName in sheetDf.columns.values:
          column_series = sheetDf.loc[~np.isnan(sheetDf[columnName]), columnName]
          column_median = column_series.median()
          column_val_counts = column_series.value_counts()
          column_mode = column_val_counts.idxmax()
          column_freq = column_val_counts.max()
          column_ci = pg.compute_bootci(column_series,func='mean')
          modes.append(column_mode)
          freqs.append(column_freq)
          medians.append(column_median)
          low_cis.append(column_ci[0])
          high_cis.append(column_ci[1])
        description.loc['mode'] = modes
        description.loc['freq'] = freqs
        description.loc['median'] = medians
        description.loc['cil'] = low_cis
        description.loc['cih'] = high_cis
        return description

    @staticmethod
    def normalityAsumption(sheetDf):
        normalityDf = pd.DataFrame(columns=sheetDf.columns.values)
        statistics = []
        pvalues = []
        areNormal = []
        for columnName in sheetDf.columns.values:
            normality_stats = pg.normality(sheetDf.loc[~np.isnan(sheetDf[columnName]), columnName])
            statistics.append(normality_stats['W'].values[0])
            pvalues.append(normality_stats['pval'].values[0])
            areNormal.append(normality_stats['normal'].values[0])
        normalityDf.loc['stat'] = statistics
        normalityDf.loc['pval'] = pvalues
        normalityDf.loc['norm'] = areNormal
        return normalityDf
       
    @staticmethod
    def plotQuantMonoFactor(
        df_quant,
        figure_size = (2, 1),
        figure_dpi = 300,
        table_bbox = [1.0, -0.25, 0.6, 1.25],
        table_head_height = 0.5,
        table_row_height = 1,
        table_head_size = 5,
        table_cell_size = 4,
        factor1_columns = [],
        factor1_labels = [],
        x_lim = None,
        variable_column = None,
        variable_label = None,
        df_benchmark = None
        ) -> plt.Figure:
        df_variable = df_quant.loc[::, ['factor0', variable_column]]
        # compute useful values
        y_min = -0.5
        y_max = ( len(factor1_columns)  - 1) + 0.5

        if x_lim == None:
            var_mean_value = df_variable[variable_column].mean()
            var_std_value = df_variable[variable_column].std()
            sigma_steps = 0.5
            x_min = var_mean_value - sigma_steps * var_std_value
            x_max =var_mean_value + sigma_steps * var_std_value
        else: 
            x_min = x_lim[0]
            x_max = x_lim[1]

        x_range = x_max-x_min
        # create figure
        offset_fraction = 0.53
        axis_label_size = 6
        fig_title_size = 6
        variable_mean_dot_size = 4
        axis_line_width = 0.5
        variable_line_width = 0.75
        variable_dot_width = 4
        fig_legend_size = 4
        variable_ci_vline_offset = 0.08
        fig = plt.figure(figsize=figure_size, dpi=figure_dpi) 
        ax = plt.axes()
        ax.set_xlim([x_min, x_max])
        ax.set_ylim(y_min, y_max)
        y_ticklabels = []
        table_data = [['M', 'CI']]
        # draw benchmark
        if df_benchmark is not None:
            df_var_bench = df_benchmark.loc[[variable_column], ::]
            y = ( len(factor1_columns) - 1) / 2
            x = x_min
            color_range = sns.color_palette("RdYlGn", len(df_var_bench.columns.values))
            for bench_column, bench_color in zip(df_var_bench.columns.values, color_range):
                width = df_var_bench[bench_column].values[0]
                height = y_max - y_min
                ax.barh(
                    y= y,
                    width=width, 
                    height=height, 
                    left= x, 
                    color= bench_color)
                x += width

            leg = ax.legend(
                labels=['%s' % bench_column for bench_column in df_var_bench.columns.values],
                loc='center',
                frameon=False,
                bbox_to_anchor=(0.55, 1.05),
                fancybox=False, 
                shadow=False, 
                fontsize=fig_legend_size, 
                #borderpad=0,
                labelspacing=0,
                columnspacing=0.25,
                handlelength=1,
                handleheight=1,
                ncol=len(df_var_bench.columns.values)
            )
        for task_index, task_value in enumerate(factor1_columns):
            # fetch Factor x Task
            df_task_modality = df_variable.loc[(df_variable['factor0'] == task_value),::]
            variable_clean = df_task_modality.loc[~np.isnan(df_variable[variable_column]), variable_column]
            variable_mean = variable_clean.mean()
            #variable_ci = pg.compute_bootci(variable_clean,func='mean')
            variable_ci = stats.t.interval(alpha=0.95, df=len(variable_clean)-1, loc=np.mean(variable_clean), scale=stats.sem(variable_clean)) 
            #variable_ci = stats.norm.interval(alpha=0.95, loc=np.mean(variable_clean), scale=stats.sem(variable_clean)) 
            # fill stats dataframe
            # plot features
            i = task_index
            #tick_label = r"${0} \times {1}$".format(factor1_labels[task_index], factor2_labels[modality_index])
            tick_label = r"${0}$".format(factor1_labels[task_index])
            y_ticklabels.append(tick_label)
            print()
            table_data.append(["%0.2f" %variable_mean, "[%0.2f, %0.2f]" %(variable_ci[0], variable_ci[1])])
            
            # ax.scatter(
            #     x=df_task_modality[variable_value].values, 
            #     y=[i-0.25 for _ in range(len(df_task_modality[variable_value].values))],
            #     c=[0.5,0.5,0.5],
            #     s=1
            # )
            ax.plot(variable_mean, i, 'o', markersize=variable_dot_width, markerfacecolor='white', markeredgecolor='black', markeredgewidth=variable_line_width) 
            ax.hlines(i, variable_ci[0], variable_ci[1], colors='black', linestyles='solid', linewidth=variable_line_width)
            ax.vlines(variable_ci[0], i-variable_ci_vline_offset, i+variable_ci_vline_offset, colors='black', linestyles='solid', linewidth=variable_line_width)
            ax.vlines(variable_ci[1], i-variable_ci_vline_offset, i+variable_ci_vline_offset, colors='black', linestyles='solid', linewidth=variable_line_width)
        ax.text(
                x = x_min - offset_fraction * x_range, 
                y = (len(factor1_columns) - 1)/2,
                ha = 'center',
                va = 'center',
                s = "Test",
                c="white",
                fontsize=axis_label_size
            )
        ax.set_yticks(range(len(y_ticklabels)))  
        ax.set_yticklabels(y_ticklabels)
        plt.title(variable_label, fontsize=fig_title_size, fontweight='bold')
        ax.tick_params(axis='y', labelsize=axis_label_size)
        ax.tick_params(axis='x', labelsize=axis_label_size)
        #ax.spines['right'].set_visible(False)
        #ax.spines['top'].set_visible(False)
        # change axes and ticks width
        for axis in ['top','bottom','left','right']:
            ax.spines[axis].set_linewidth(axis_line_width)
        ax.xaxis.set_tick_params(width=axis_line_width)
        ax.yaxis.set_tick_params(width=axis_line_width)
        # draw table
        axes_table = tab.Table(
            ax,
            bbox = table_bbox,
            #loc=trans.Bbox([[0, 1], [1, 7]]) #trans.Bbox.from_extents(x_max, y_max, 3, 7)
            )
            
        axes_table.auto_set_font_size(False)
        visible_edges_value = "open"
        nb_row = len(table_data)
        nb_col = len(table_data[0])
        for row_index, row_values in enumerate(table_data):
                for col_index, col_value in enumerate(row_values):
                    col_width = 0.5 if col_index == 0 else 1
                    if row_index == 0:
                            cell = axes_table.add_cell(
                                row= (nb_row - 1) - row_index,
                                col= col_index, 
                                width=col_width,
                                height=table_head_height,
                                edgecolor='k',
                                facecolor='w',
                                fill=False,
                                text=col_value,
                                loc='center',
                                fontproperties=FontProperties(size=table_head_size, weight='bold')
                            )
                            cell.visible_edges = "open"
                            cell.set(linewidth=axis_line_width)
                    else:
                        cell = axes_table.add_cell(
                                row= (nb_row - 1) - row_index,
                                col=col_index, 
                                width=col_width,
                                height=table_row_height,
                                edgecolor='k',
                                facecolor='w',
                                fill=False,
                                text=col_value,
                                loc='center',
                                fontproperties=FontProperties(size=table_cell_size)
                            )
                        cell.visible_edges = "LR"
                        cell.set(linewidth=axis_line_width)
        ax.add_table(axes_table)
        return fig

    @staticmethod
    def plotQuantDualFactors(
        df_quant,
        figure_size = (2, 1),
        figure_dpi = 300,
        table_bbox = [1.0, -0.12, 0.6, 1.12],
        table_head_height = 0.5,
        table_row_height = 1,
        table_head_size = 5,
        table_cell_size = 4,
        factor1_columns = [],
        factor1_labels = [],
        factor2_columns = [],
        factor2_labels = [],
        x_lim = None,
        variable_column = None,
        variable_label = None,
        df_benchmark = None
        ) -> plt.Figure:
        # fetch variable dataframe
        df_variable = df_quant.loc[::, ['factor0', 'factor1', variable_column]]
        # compute useful values
        y_min = -0.5
        y_max = ( len(factor1_columns) * len(factor2_columns) - 1) + 0.5

        if x_lim == None:
            var_mean_value = df_variable[variable_column].mean()
            var_std_value = df_variable[variable_column].std()
            sigma_steps = 0.5
            x_min = var_mean_value - sigma_steps * var_std_value
            x_max =var_mean_value + sigma_steps * var_std_value
        else: 
            x_min = x_lim[0]
            x_max = x_lim[1]

        x_range = x_max-x_min
        # create figure
        offset_fraction = 0.53
        axis_label_size = 6
        fig_title_size = 6
        variable_mean_dot_size = 4
        axis_line_width = 0.5
        variable_line_width = 0.75
        variable_dot_width = 4
        fig_legend_size = 4
        variable_ci_vline_offset = 0.16
        fig = plt.figure(figsize=figure_size, dpi=figure_dpi) 
        ax = plt.axes()
        ax.set_xlim([x_min, x_max])
        ax.set_ylim(y_min, y_max)
        y_ticklabels = []
        table_data = [['M', 'CI']]
        # draw benchmark
        if df_benchmark is not None:
            df_var_bench = df_benchmark.loc[[variable_column], ::]
            y = ( len(factor1_columns) * len(factor2_columns) - 1) / 2
            x = x_min
            color_range = sns.color_palette("RdYlGn", len(df_var_bench.columns.values))
            for bench_column, bench_color in zip(df_var_bench.columns.values, color_range):
                width = df_var_bench[bench_column].values[0]
                height = y_max - y_min
                ax.barh(
                    y= y,
                    width=width, 
                    height=height, 
                    left= x, 
                    color= bench_color)
                x += width

            leg = ax.legend(
                labels=['%s' % bench_column for bench_column in df_var_bench.columns.values],
                loc='center',
                frameon=False,
                bbox_to_anchor=(0.55, 1.05),
                fancybox=False, 
                shadow=False, 
                fontsize=fig_legend_size, 
                #borderpad=0,
                labelspacing=0,
                columnspacing=0.25,
                handlelength=1,
                handleheight=1,
                ncol=len(df_var_bench.columns.values)
            )
        for task_index, task_value in enumerate(factor1_columns):
            for modality_index, modality_value in enumerate(factor2_columns):
                # fetch Factor x Task
                df_task_modality = df_variable.loc[(df_variable['factor0'] == task_value) & (df_variable['factor1'] == modality_value),::]
                variable_clean = df_task_modality.loc[~np.isnan(df_variable[variable_column]), variable_column]
                variable_mean = variable_clean.mean()
                #variable_ci = pg.compute_bootci(variable_clean,func='mean')
                variable_ci = stats.t.interval(alpha=0.95, df=len(variable_clean)-1, loc=np.mean(variable_clean), scale=stats.sem(variable_clean)) 
                #variable_ci = stats.norm.interval(alpha=0.95, loc=np.mean(variable_clean), scale=stats.sem(variable_clean)) 
                # fill stats dataframe
                # plot features
                i = task_index * len(factor2_columns) + modality_index
                #tick_label = r"${0} \times {1}$".format(factor1_labels[task_index], factor2_labels[modality_index])
                tick_label = r"${0}$".format(factor2_labels[modality_index])
                y_ticklabels.append(tick_label)
                table_data.append(["%0.2f" %variable_mean, "[%0.2f, %0.2f]" %(variable_ci[0], variable_ci[1])])
        
                # ax.scatter(
                #     x=df_task_modality[variable_value].values, 
                #     y=[i-0.25 for _ in range(len(df_task_modality[variable_value].values))],
                #     c=[0.5,0.5,0.5],
                #     s=1
                # )
                ax.plot(variable_mean, i, 'o', markersize=variable_dot_width, markerfacecolor='white', markeredgecolor='black', markeredgewidth=variable_line_width) 
                ax.hlines(i, variable_ci[0], variable_ci[1], colors='black', linestyles='solid', linewidth=variable_line_width)
                ax.vlines(variable_ci[0], i-variable_ci_vline_offset, i+variable_ci_vline_offset, colors='black', linestyles='solid', linewidth=variable_line_width)
                ax.vlines(variable_ci[1], i-variable_ci_vline_offset, i+variable_ci_vline_offset, colors='black', linestyles='solid', linewidth=variable_line_width)

            ax.text(
                x = x_min - offset_fraction * x_range, 
                y = (len(factor2_labels) - 1)/2 + task_index * len(factor2_labels),
                ha = 'center',
                va = 'center',
                #c = '#7e7e7e',
                fontsize=axis_label_size,
                s = r"${0}$".format(factor1_labels[task_index])
            )
        ax.set_yticks(range(len(y_ticklabels)))  
        ax.set_yticklabels(y_ticklabels)
        plt.title(variable_label, fontsize=fig_title_size, fontweight='bold')
        ax.tick_params(axis='y', labelsize=axis_label_size)
        ax.tick_params(axis='x', labelsize=axis_label_size)
        #ax.spines['right'].set_visible(False)
        #ax.spines['top'].set_visible(False)
            # change axes and ticks width
        for axis in ['top','bottom','left','right']:
            ax.spines[axis].set_linewidth(axis_line_width)
        ax.xaxis.set_tick_params(width=axis_line_width)
        ax.yaxis.set_tick_params(width=axis_line_width)
        # draw table
        axes_table = tab.Table(
            ax,
            bbox = table_bbox,
            #loc=trans.Bbox([[0, 1], [1, 7]]) #trans.Bbox.from_extents(x_max, y_max, 3, 7)
            )
            
        axes_table.auto_set_font_size(False)
        visible_edges_value = "open"
        nb_row = len(table_data)
        nb_col = len(table_data[0])
        for row_index, row_values in enumerate(table_data):
                for col_index, col_value in enumerate(row_values):
                    col_width = 0.5 if col_index == 0 else 1
                    if row_index == 0:
                            cell = axes_table.add_cell(
                                row= (nb_row - 1) - row_index,
                                col= col_index, 
                                width=col_width,
                                height=table_head_height,
                                edgecolor='k',
                                facecolor='w',
                                fill=False,
                                text=col_value,
                                loc='center',
                                fontproperties=FontProperties(size=table_head_size, weight='bold')
                            )
                            cell.visible_edges = "open"
                            cell.set(linewidth=axis_line_width)
                    else:
                        cell = axes_table.add_cell(
                                row= (nb_row - 1) - row_index,
                                col=col_index, 
                                width=col_width,
                                height=table_row_height,
                                edgecolor='k',
                                facecolor='w',
                                fill=False,
                                text=col_value,
                                loc='center',
                                fontproperties=FontProperties(size=table_cell_size)
                            )
                        cell.visible_edges = "LR"
                        cell.set(linewidth=axis_line_width)
        ax.add_table(axes_table)
        return fig

    @staticmethod
    def plotOrdinalMonoFactor(
        df_qual,
        figure_size = (2, 1),
        figure_dpi = 300,
        legend_hide = False,
        legend_bbox_anchor = (0.55, 1.07),
        table_bbox = [1.00, -0.25, 0.25, 1.25],
        table_head_height = 0.5,
        table_row_height = 1,
        table_column_width = 1,
        table_head_size = 5,
        table_cell_size = 4,
        factor_columns = [],
        factor_value_label_map = {},
        factor1_columns = [],
        factor1_labels = [],
        variable_column = None,
        variable_label = None,
        variable_lim = (0,5),
        ) -> plt.Figure:
        # fetch variable dataframe
        df_variable = df_qual.loc[::, ['factor0', variable_column]]
        x_min = -100
        x_max = 100
        x_range = x_max - x_min
        # create figure
        offset_fraction = 0.5
        axis_label_size = 6
        fig_title_size = 6
        fig_title_pad = 8
        fig_legend_size = 5
        axis_line_width = 0.5
        annotation_label_size = 5
        height=0.75
        fig = plt.figure(figsize=figure_size, dpi=figure_dpi) 
        ax = plt.axes()
        var_min = variable_lim[0]
        var_max = variable_lim[1]
        var_range = var_max - var_min
        var_range_list = range(var_min, var_max + 1)
        nb_column = var_range + 1
        column_indexes = np.arange(nb_column)    # the x locations for the groups
        color_range = sns.color_palette("RdYlGn", nb_column)

        nb_row = len(factor1_columns)
        row_indexes = np.arange(nb_row)    # the x locations for the groups
        row_labels = []
        # getting middle position
        xMidPos = 0
        data = {}
        for index in range(nb_column):
            data[index] = {'x':[], 'y':[],  'label':[], 'count':[], 'width':[], 'height':[], 'left':[], 'color':[], 'edgecolor':[]}
        table_data = [["M", "MD"]]
        for task_index, task_value in enumerate(factor1_columns):
                # fetch Factor x Task
                df_task_modality = df_variable.loc[(df_variable['factor0'] == task_value),::]
                df_task_modality_hist = pd.crosstab(index=df_task_modality[variable_column], columns=df_task_modality['factor0'])
                df_task_modality_hist.rename(columns={ df_task_modality_hist.columns[0]: "Count" }, inplace = True)
                for i in range(var_min, var_max+1):
                    if i not in df_task_modality_hist.index.values:
                        df_task_modality_hist.loc[i] = [0]
                df_task_modality_hist.sort_index(ascending=True, inplace=True)
                row_index = task_index
                #columnName = r"${0} \wedge {1}$".format(factor1_labels[task_index], factor2_labels[modality_index]) #task_value + "-" + modality_value 
                columnName = r"${0}$".format(factor1_labels[task_index]) #task_value + "-" + modality_value 
                row_labels.append(columnName)
                df_raw = df_task_modality_hist.loc[:,'Count']
                df_percent = df_raw/df_raw.sum() * 100
                # compute stats
                variable_mean = df_task_modality[variable_column].mean()
                variable_median = df_task_modality[variable_column].median()
                #variable_mode = df_task_modality[variable_column].mode() , '/'.join([str(mode) for mode in variable_mode])
                table_data.append(['%0.2f' %variable_mean, '%0.2f' %variable_median])
                if nb_column % 2: # unpaired
                    midIndex = int(nb_column/2)
                    leftMidPos = xMidPos - df_percent.iloc[midIndex]/2
                    data[midIndex]['label'].append(columnName)
                    data[midIndex]['width'].append(df_percent.iloc[midIndex])
                    data[midIndex]['count'].append(df_raw.iloc[midIndex])
                    data[midIndex]['left'].append(leftMidPos)
                    data[midIndex]['color'].append(color_range[midIndex])
                    data[midIndex]['y'].append(row_index)
                    data[midIndex]['x'].append(leftMidPos + df_percent.iloc[midIndex]/2)

                    for j in range(midIndex-1, -1, -1):
                        leftLeftPos = leftMidPos - df_percent.iloc[j:midIndex].sum()
                        data[j]['label'].append(columnName)
                        data[j]['width'].append(df_percent.iloc[j])
                        data[j]['count'].append(df_raw.iloc[j])
                        data[j]['left'].append(leftLeftPos)
                        data[j]['color'].append(color_range[j])
                        data[j]['y'].append(row_index)
                        data[j]['x'].append(leftLeftPos + df_percent.iloc[j]/2)

                    for j in range(midIndex+1, len(df_percent.index.values), +1):
                        leftRightPos = leftMidPos + df_percent.iloc[midIndex:j].sum()
                        data[j]['label'].append(columnName)
                        data[j]['width'].append(df_percent.iloc[j])
                        data[j]['count'].append(df_raw.iloc[j])
                        data[j]['left'].append(leftRightPos)
                        data[j]['color'].append(color_range[j])
                        data[j]['y'].append(row_index)
                        data[j]['x'].append(leftRightPos + df_percent.iloc[j]/2)


                else: # paired
                    midIndex = int(nb_column/2)
                    for j in range(midIndex-1, -1, -1):
                        leftLeftPos = xMidPos - df_percent.iloc[j:midIndex].sum()
                        data[j]['label'].append(columnName)
                        data[j]['width'].append(df_percent.iloc[j])
                        data[j]['count'].append(df_raw.iloc[j])
                        data[j]['left'].append(leftLeftPos)
                        data[j]['color'].append(color_range[j])
                        data[j]['y'].append(row_index)
                        data[j]['x'].append(leftLeftPos + df_percent.iloc[j]/2)

                    for j in range(midIndex, len(df_percent.index.values), +1):
                        leftRightPos = xMidPos + df_percent.iloc[midIndex:j].sum()
                        data[j]['label'].append(columnName)
                        data[j]['width'].append(df_percent.iloc[j])
                        data[j]['count'].append(df_raw.iloc[j])
                        data[j]['left'].append(leftRightPos)
                        data[j]['color'].append(color_range[j])
                        data[j]['y'].append(row_index)
                        data[j]['x'].append(leftRightPos + df_percent.iloc[j]/2)
        ax.text(
                x = x_min - offset_fraction * x_range, 
                y = (len(factor1_columns) - 1)/2,
                ha = 'center',
                va = 'center',
                s = "Test",
                c="white",
                fontsize=axis_label_size
            )
        for key, value in data.items():
            ax.barh(y=value['label'], width=value['width'], height=height, left=value['left'], color=value['color'])
            for x, y, v, c in zip(value['x'], value['y'], value['count'], value['color']):
                if int(v):
                    ax.text(x, y, int(v), ha='center', va='center',  size=annotation_label_size, color='black')
        if not legend_hide:
            leg = ax.legend(
                labels=['%s pts' % var_range_list[key] for key in data],
                loc='center',
                frameon=False,
                bbox_to_anchor=legend_bbox_anchor,
                fancybox=False, 
                shadow=False, 
                fontsize=fig_legend_size, 
                #borderpad=0,
                labelspacing=0,
                columnspacing=0.25,
                handlelength=1,
                handleheight=1,
                ncol=len(data)
                )
            for txt in  leg.get_texts():
                txt.set_ha("left") # horizontal alignment of text item
                txt.set_x(-10) # x-position
                #txt.set_y(10) # y-position

        ax.set_xlabel('Frequency', size=axis_label_size)
        ax.set_yticks(row_indexes)
        ax.set_yticklabels(row_labels)
        ax.set_xlim([x_min, x_max])
        vals = ax.get_xticks()
        
        ticks_loc = ax.get_xticks().tolist()
        ax.xaxis.set_major_locator(mticker.FixedLocator(ticks_loc))
        ax.set_xticklabels([ str(int(abs(x))) + '%' for x in vals])
        # Get ticklabels for fixed ticks
        ax_xticklabels = ax.get_xticklabels()
        # set the alignment for outer ticklabels
        ax_xticklabels[0].set_ha("left")
        ax_xticklabels[-1].set_ha("right")
        ax.tick_params(axis='y', labelsize=axis_label_size)
        ax.tick_params(axis='x', labelsize=axis_label_size)
        #ax.spines['right'].set_visible(False)
        #ax.spines['top'].set_visible(False)

        # change axes and ticks width
        for axis in ['top','bottom','left','right']:
            ax.spines[axis].set_linewidth(axis_line_width)
        ax.xaxis.set_tick_params(width=axis_line_width)
        ax.yaxis.set_tick_params(width=axis_line_width)
        # draw table
        axis_line_width = 0.5
        axes_table = tab.Table(
            ax,
            bbox = table_bbox 
            #loc=trans.Bbox([[0, 1], [1, 7]]) #trans.Bbox.from_extents(x_max, y_max, 3, 7)
            )
        axes_table.auto_set_font_size(False)
        axes_table.auto_set_font_size(False)
        visible_edges_value = "open" #"RLTB"
        nb_row = len(table_data)
        nb_col = len(table_data[0])
        for row_index, row_values in enumerate(table_data):
                for col_index, col_value in enumerate(row_values):
                    col_width = table_column_width #len(table_data[0][col_index])
                    if row_index == 0:
                            cell = axes_table.add_cell(
                                row= (nb_row - 1) - row_index,
                                col= col_index, 
                                width=col_width,
                                height=table_head_height,
                                edgecolor='k',
                                facecolor='w',
                                fill=False,
                                text=col_value,
                                loc='center',
                                fontproperties=FontProperties(size=table_head_size, style="normal", weight="bold")
                            )
                            # if col_index == 0:
                            #     cell.visible_edges = "LTB"
                            # if col_index == nb_col - 1:
                            #     cell.visible_edges = "RTB"
                            cell.visible_edges = "open"
                            cell.set(linewidth=axis_line_width)
                    else:
                        cell = axes_table.add_cell(
                                row= (nb_row - 1) - row_index,
                                col=col_index, 
                                width=col_width,
                                height=table_row_height,
                                edgecolor='k',
                                facecolor='w',
                                fill=False,
                                text=col_value,
                                loc='center',
                                fontproperties=FontProperties(size=table_cell_size)
                            )
                        # if col_index == 0:
                        #     cell.visible_edges = "LTB"
                        # if col_index == nb_col - 1:
                        #     cell.visible_edges = "RTB"
                        cell.visible_edges = "LR" #visible_edges_value
                        cell.set(linewidth=axis_line_width)
    
        ax.add_table(axes_table)
        plt.title(variable_label, fontsize=fig_title_size, pad=fig_title_pad, fontweight='bold')

        return fig

    @staticmethod
    def plotOrdinalDualFactors(
        df_qual,
        figure_size = (2, 1),
        figure_dpi = 300,
        legend_bbox_anchor = (0.55, 1.07),
        legend_hide = False,
        table_bbox = [1.00, -0.25, 0.25, 1.25],
        table_head_height = 1,
        table_row_height = 1,
        table_column_width = 1,
        table_head_size = 5,
        table_cell_size = 4,
        factor_columns = [],
        factor_value_label_map = {},
        factor1_columns = [],
        factor1_labels = [],
        factor2_columns = [],
        factor2_labels = [],
        variable_column = None,
        variable_label = None,
        variable_lim = (0,5),
        ) -> plt.Figure:
        # fetch variable dataframe
        df_variable = df_qual.loc[::, ['factor0', 'factor1', variable_column]]
        x_min = -100
        x_max = 100
        x_range = x_max - x_min
        # create figure
        offset_fraction = 0.5
        axis_label_size = 6
        fig_title_size = 6
        fig_title_pad = 8
        fig_legend_size = 5
        axis_line_width = 0.5
        annotation_label_size = 5
        table_data = [["M", "MD"]]
        height=0.75
        fig = plt.figure(figsize=figure_size, dpi=figure_dpi) 
        ax = plt.axes()
        var_min = variable_lim[0]
        var_max = variable_lim[1]
        var_range = var_max - var_min
        var_range_list = range(var_min, var_max + 1)
        nb_column = var_range + 1
        column_indexes = np.arange(nb_column)    # the x locations for the groups
        color_range = sns.color_palette("RdYlGn", nb_column)

        nb_row = len(factor1_columns) * len(factor2_columns)
        row_indexes = np.arange(nb_row)    # the x locations for the groups
        row_labels = []
        # getting middle position
        xMidPos = 0
        data = {}
        for index in range(nb_column):
            data[index] = {'x':[], 'y':[],  'label':[], 'count':[], 'width':[], 'height':[], 'left':[], 'color':[], 'edgecolor':[]}
        
        for task_index, task_value in enumerate(factor1_columns):
            for modality_index, modality_value in enumerate(factor2_columns):
                # fetch Factor x Task
                df_task_modality = df_variable.loc[(df_variable['factor0'] == task_value) & (df_variable['factor1'] == modality_value),::]
                df_task_modality_hist = pd.crosstab(index=df_task_modality[variable_column], columns=df_task_modality['factor0'])
                df_task_modality_hist.rename(columns={ df_task_modality_hist.columns[0]: "Count" }, inplace = True)
                for i in range(var_min, var_max+1):
                    if i not in df_task_modality_hist.index.values:
                        df_task_modality_hist.loc[i] = [0]
                df_task_modality_hist.sort_index(ascending=True, inplace=True)
                row_index = task_index * len(factor2_columns) + modality_index
                #columnName = r"${0} \wedge {1}$".format(factor1_labels[task_index], factor2_labels[modality_index]) #task_value + "-" + modality_value 
                columnName = r"${1}{0}$".format(' ' * row_index, factor2_labels[modality_index]) #task_value + "-" + modality_value 
                row_labels.append(columnName)
                df_raw = df_task_modality_hist.loc[:,'Count']
                df_percent = df_raw/df_raw.sum() * 100
                # compute stats
                variable_mean = df_task_modality[variable_column].mean()
                variable_median = df_task_modality[variable_column].median()
                table_data.append(['%0.2f' %variable_mean, '%0.2f' %variable_median])
                if nb_column % 2: # unpaired
                    midIndex = int(nb_column/2)
                    leftMidPos = xMidPos - df_percent.iloc[midIndex]/2
                    data[midIndex]['label'].append(columnName)
                    data[midIndex]['width'].append(df_percent.iloc[midIndex])
                    data[midIndex]['count'].append(df_raw.iloc[midIndex])
                    data[midIndex]['left'].append(leftMidPos)
                    data[midIndex]['color'].append(color_range[midIndex])
                    data[midIndex]['y'].append(row_index)
                    data[midIndex]['x'].append(leftMidPos + df_percent.iloc[midIndex]/2)

                    for j in range(midIndex-1, -1, -1):
                        leftLeftPos = leftMidPos - df_percent.iloc[j:midIndex].sum()
                        data[j]['label'].append(columnName)
                        data[j]['width'].append(df_percent.iloc[j])
                        data[j]['count'].append(df_raw.iloc[j])
                        data[j]['left'].append(leftLeftPos)
                        data[j]['color'].append(color_range[j])
                        data[j]['y'].append(row_index)
                        data[j]['x'].append(leftLeftPos + df_percent.iloc[j]/2)

                    for j in range(midIndex+1, len(df_percent.index.values), +1):
                        leftRightPos = leftMidPos + df_percent.iloc[midIndex:j].sum()
                        data[j]['label'].append(columnName)
                        data[j]['width'].append(df_percent.iloc[j])
                        data[j]['count'].append(df_raw.iloc[j])
                        data[j]['left'].append(leftRightPos)
                        data[j]['color'].append(color_range[j])
                        data[j]['y'].append(row_index)
                        data[j]['x'].append(leftRightPos + df_percent.iloc[j]/2)


                else: # paired
                    midIndex = int(nb_column/2)
                    for j in range(midIndex-1, -1, -1):
                        leftLeftPos = xMidPos - df_percent.iloc[j:midIndex].sum()
                        data[j]['label'].append(columnName)
                        data[j]['width'].append(df_percent.iloc[j])
                        data[j]['count'].append(df_raw.iloc[j])
                        data[j]['left'].append(leftLeftPos)
                        data[j]['color'].append(color_range[j])
                        data[j]['y'].append(row_index)
                        data[j]['x'].append(leftLeftPos + df_percent.iloc[j]/2)

                    for j in range(midIndex, len(df_percent.index.values), +1):
                        leftRightPos = xMidPos + df_percent.iloc[midIndex:j].sum()
                        data[j]['label'].append(columnName)
                        data[j]['width'].append(df_percent.iloc[j])
                        data[j]['count'].append(df_raw.iloc[j])
                        data[j]['left'].append(leftRightPos)
                        data[j]['color'].append(color_range[j])
                        data[j]['y'].append(row_index)
                        data[j]['x'].append(leftRightPos + df_percent.iloc[j]/2)
            ax.text(
                x = x_min - offset_fraction * x_range, 
                y = (len(factor2_columns) - 1)/2 + task_index * len(factor2_columns),
                ha = 'center',
                va = 'center',
                s = r"${0}$".format(factor1_labels[task_index]),
                fontsize=axis_label_size
            )

        for key, value in data.items():
            ax.barh(y=value['label'], width=value['width'], height=height, left=value['left'], color=value['color'])
            for x, y, v, c in zip(value['x'], value['y'], value['count'], value['color']):
                if int(v):
                    ax.text(x, y, int(v), ha='center', va='center',  size=6, color='black')
        if not legend_hide:
            leg = ax.legend(
                labels=['%s pts' % var_range_list[key] for key in data],
                loc='center',
                frameon=False,
                bbox_to_anchor=legend_bbox_anchor,
                fancybox=False, 
                shadow=False, 
                fontsize=fig_legend_size, 
                #borderpad=0,
                labelspacing=0,
                columnspacing=0.25,
                handlelength=1,
                handleheight=1,
                ncol=len(data)
                )
            for txt in  leg.get_texts():
                txt.set_ha("left") # horizontal alignment of text item
                txt.set_x(-10) # x-position
                #txt.set_y(10) # y-position

        ax.set_xlabel('Frequency', size=axis_label_size)
        ax.set_yticks(row_indexes)
        ax.set_yticklabels(row_labels)
        ax.set_xlim([x_min, x_max])
        vals = ax.get_xticks()
        
        ticks_loc = ax.get_xticks().tolist()
        ax.xaxis.set_major_locator(mticker.FixedLocator(ticks_loc))
        ax.set_xticklabels([ str(int(abs(x))) + '%' for x in vals])
        # Get ticklabels for fixed ticks
        ax_xticklabels = ax.get_xticklabels()
        # set the alignment for outer ticklabels
        ax_xticklabels[0].set_ha("left")
        ax_xticklabels[-1].set_ha("right")
        ax.tick_params(axis='y', labelsize=axis_label_size)
        ax.tick_params(axis='x', labelsize=axis_label_size)
        #ax.spines['right'].set_visible(False)
        #ax.spines['top'].set_visible(False)

        # change axes and ticks width
        for axis in ['top','bottom','left','right']:
            ax.spines[axis].set_linewidth(axis_line_width)
        ax.xaxis.set_tick_params(width=axis_line_width)
        ax.yaxis.set_tick_params(width=axis_line_width)
        # draw table
        axis_line_width = 0.5
        axes_table = tab.Table(
            ax,
            bbox = table_bbox
            #loc=trans.Bbox([[0, 1], [1, 7]]) #trans.Bbox.from_extents(x_max, y_max, 3, 7)
            )
        axes_table.auto_set_font_size(False)
        axes_table.auto_set_font_size(False)
        visible_edges_value = "open" #"RLTB"
        nb_row = len(table_data)
        nb_col = len(table_data[0])
        for row_index, row_values in enumerate(table_data):
                for col_index, col_value in enumerate(row_values):
                    col_width = table_column_width #len(table_data[0][col_index])
                    if row_index == 0:
                            cell = axes_table.add_cell(
                                row= (nb_row - 1) - row_index,
                                col= col_index, 
                                width=col_width,
                                height=table_head_height,
                                edgecolor='k',
                                facecolor='w',
                                fill=False,
                                text=col_value,
                                loc='center',
                                fontproperties=FontProperties(size=table_head_size, style="normal", weight="bold")
                            )
                            # if col_index == 0:
                            #     cell.visible_edges = "LTB"
                            # if col_index == nb_col - 1:
                            #     cell.visible_edges = "RTB"
                            cell.visible_edges = "open"
                            cell.set(linewidth=axis_line_width)
                    else:
                        cell = axes_table.add_cell(
                                row= (nb_row - 1) - row_index,
                                col=col_index, 
                                width=col_width,
                                height=table_row_height,
                                edgecolor='k',
                                facecolor='w',
                                fill=False,
                                text=col_value,
                                loc='center',
                                fontproperties=FontProperties(size=table_cell_size)
                            )
                        # if col_index == 0:
                        #     cell.visible_edges = "LTB"
                        # if col_index == nb_col - 1:
                        #     cell.visible_edges = "RTB"
                        cell.visible_edges = "LR" #visible_edges_value
                        cell.set(linewidth=axis_line_width)
    
        ax.add_table(axes_table)
        plt.title(variable_label, fontsize=fig_title_size, pad=fig_title_pad, fontweight='bold')

        return fig

    @staticmethod
    def inferQualOrdinalPaired(sheetDf, sheetRange) -> pd.DataFrame:
        meltedSheetDf = sheetDf.melt(var_name='factor', value_name='variable')
        contingencySheetDf = pd.crosstab(index=meltedSheetDf['variable'], columns=meltedSheetDf['factor'])
        statDf = pd.DataFrame(columns=['Comparison', 'N', 'Test', 'Statistic', 'pValue', 'effectSize'])
        #fill empty scale value
        for sheetStep in range(sheetRange[0], sheetRange[1] + 1):
            if not sheetStep in contingencySheetDf.index.values:
                contingencySheetDf.loc[sheetStep] = [0 for x in range(len(contingencySheetDf.columns.values))]
        contingencySheetDf.sort_index(inplace=True)

        # ALL MODALITY
        if len(contingencySheetDf.columns) > 2:
            sheetDf_long = sheetDf.melt(ignore_index=False).reset_index()
            friedman_stats = pg.friedman(data=sheetDf_long, dv="value", within="variable", subject="index")
            source, wvalue, ddof1, qvalue, pvalue = friedman_stats.values[0]
            statDf = statDf.append(
                {'Comparison': sheetDf.columns.str.cat(sep=' | '),
                'N': sheetDf[sheetDf.columns.values[0]].size,
                'Test': "Friedman",
                'Statistic':qvalue,
                'pValue':pvalue,
                'effectSize':wvalue
                }, ignore_index=True)

        # BETWEEN MODALITY
        modality_names = sheetDf.columns.values
        uncorrectedStatIndex = len(statDf.index)
        for i in range(len(modality_names)):
            for j in range(i+1, len(modality_names)):
                if Statistics.areRelatedFactors(modality_names[i], modality_names[j]):
                    stats_wilcoxon = pg.wilcoxon(sheetDf.loc[:, modality_names[i]], sheetDf.loc[:, modality_names[j]],  alternative='two-sided')
                    wvalue, alternative, pvalue, RBC, CLES = stats_wilcoxon.values[0]
                    statDf = statDf.append(
                        {
                            'Comparison': modality_names[i] + '|' + modality_names[j],
                            'N': sheetDf[sheetDf.columns.values[i]].size,
                            'Test': "Wilcoxon",
                            'Statistic':wvalue,
                            'pValue':pvalue,
                            'effectSize': RBC
                        }, ignore_index=True)
        uncorrected_pvalues = np.array(statDf.loc[uncorrectedStatIndex::,'pValue'].values, dtype=np.float64)
        reject, statDf.loc[uncorrectedStatIndex::,'pValue'] = pg.multicomp(uncorrected_pvalues, alpha=0.05, method="bonf")
        return statDf
        
    @staticmethod
    def plotQuantPaired(
        df_quant,
        figure_size = (2, 1),
        figure_dpi = 300,
        factor1_columns = [],
        factor1_labels = [],
        factor2_columns = [],
        factor2_labels = [],
        x_lim = None,
        variable_column = None,
        variable_label = None,
        df_benchmark = None
        ) -> plt.Figure:
         # fetch variable dataframe
        df_variable = df_quant.loc[::, ['Task', 'Modality', variable_column]]
        # compute useful values
        y_min = -0.5
        y_max = ( len(factor1_columns) * len(factor2_columns) - 1) + 0.5

        if x_lim == None:
            var_mean_value = df_variable[variable_column].mean()
            var_std_value = df_variable[variable_column].std()
            sigma_steps = 0.5
            x_min = var_mean_value - sigma_steps * var_std_value
            x_max =var_mean_value + sigma_steps * var_std_value
        else: 
            x_min = x_lim[0]
            x_max = x_lim[1]

        x_range = x_max-x_min
        # create figure
        offset_fraction = 0.5
        axis_label_size = 6
        fig_title_size = 7
        fig_legend_size = 5
        fig = plt.figure(figsize=figure_size, dpi=figure_dpi) 
        ax = plt.axes()
        ax.set_xlim([x_min, x_max])
        ax.set_ylim(y_min, y_max)
        y_ticklabels = []
        # draw benchmark
        if df_benchmark is not None:
            df_var_bench = df_benchmark.loc[[variable_column], ::]
            y = ( len(factor1_columns) * len(factor2_columns) - 1) / 2
            x = x_min
            color_range = sns.color_palette("RdYlGn", len(df_var_bench.columns.values))
            for bench_column, bench_color in zip(df_var_bench.columns.values, color_range):
                width = df_var_bench[bench_column].values[0]
                height = y_max - y_min
                ax.barh(
                    y= y,
                    width=width, 
                    height=height, 
                    left= x, 
                    color= bench_color)
                x += width

            leg = ax.legend(
                labels=['%s' % bench_column for bench_column in df_var_bench.columns.values],
                loc='center',
                frameon=False,
                bbox_to_anchor=(0.55, 1.05),
                fancybox=False, 
                shadow=False, 
                fontsize=fig_legend_size, 
                #borderpad=0,
                labelspacing=0,
                columnspacing=0.25,
                handlelength=1,
                handleheight=1,
                ncol=len(df_var_bench.columns.values)
            )
        for task_index, task_value in enumerate(factor1_columns):
            for modality_index, modality_value in enumerate(factor2_columns):
                # fetch Factor x Task
                df_task_modality = df_variable.loc[(df_variable['Task'] == task_value) & (df_variable['Modality'] == modality_value),::]
                variable_clean = df_task_modality.loc[~np.isnan(df_variable[variable_column]), variable_column]
                variable_mean = variable_clean.mean()
                #variable_ci = pg.compute_bootci(variable_clean,func='mean')
                variable_ci = stats.t.interval(alpha=0.95, df=len(variable_clean)-1, loc=np.mean(variable_clean), scale=stats.sem(variable_clean)) 
                #variable_ci = stats.norm.interval(alpha=0.95, loc=np.mean(variable_clean), scale=stats.sem(variable_clean)) 
                # fill stats dataframe
                # plot features
                i = task_index * len(factor2_columns) + modality_index
                #tick_label = r"${0} \times {1}$".format(factor1_labels[task_index], factor2_labels[modality_index])
                tick_label = r"${0}$".format(factor2_labels[modality_index])
                y_ticklabels.append(tick_label)
                # ax.scatter(
                #     x=df_task_modality[variable_value].values, 
                #     y=[i-0.25 for _ in range(len(df_task_modality[variable_value].values))],
                #     c=[0.5,0.5,0.5],
                #     s=1
                # )
                ax.plot(variable_mean, i, 'o', markersize=5, markerfacecolor='white', markeredgecolor='black', markeredgewidth=1.25) 
                ax.hlines(i, variable_ci[0], variable_ci[1], colors='black', linestyles='solid', linewidth=1.25)
            ax.text(
                x = x_min - offset_fraction * x_range, 
                y = (len(factor2_labels) - 1)/2 + task_index * len(factor2_labels),
                ha = 'center',
                va = 'center',
                fontsize=axis_label_size,
                s = r"${0}$".format(factor1_labels[task_index])
            )
        ax.set_yticks(range(len(y_ticklabels)))  
        ax.set_yticklabels(y_ticklabels)
        plt.title(variable_label, fontsize=fig_title_size, fontweight='bold')
        ax.tick_params(axis='y', labelsize=axis_label_size)
        ax.tick_params(axis='x', labelsize=axis_label_size)
        ax.spines['right'].set_visible(False)
        ax.spines['top'].set_visible(False)
        return fig

    @staticmethod
    def inferQuantPaired(sheetDf, normality_assumption = 'auto') -> pd.DataFrame:
        normalityDf = Statistics.normalityAsumption(sheetDf)
        allColumnsAreNormal = normalityDf.loc['norm'].sum() == len(normalityDf.columns.values)
        statDf = pd.DataFrame(columns=['Comparison', 'N', 'Normality', 'Test', 'Statistic', 'pValue', 'effectSize'])
        if (normality_assumption is 'auto' and allColumnsAreNormal) or (normality_assumption is 'true'):
            if len(sheetDf.columns) > 2:
                aov = pg.rm_anova(sheetDf) # two-ways repeated-measures ANOVA
                statistic = aov['F'].values[0]
                pvalue = aov['p-GG-corr'].values[0] if 'p-GG-corr' in aov.columns.values else aov['p-unc'].values[0]
                effsize = aov['np2'].values[0]
                statDf = statDf.append(
                    {'Comparison':  sheetDf.columns.str.cat(sep=' | '),
                    'N': sheetDf[sheetDf.columns.values[0]].size,
                    'Normality':allColumnsAreNormal,
                    'Test': "ANOVA",
                    'Statistic':statistic,
                    'pValue':pvalue,
                    'effectSize':effsize
                    }, ignore_index=True)
            
            uncorrectedStatIndex = len(statDf.index)
            for i in range(len(sheetDf.columns.values)):
                for j in range(i+1, len(sheetDf.columns.values)):
                    if Statistics.areRelatedFactors(sheetDf.columns.values[i], sheetDf.columns.values[j]):
                        try:
                            df = sheetDf[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
                            statistic, pvalue = stats.ttest_ind(
                                *[
                                        df.loc[~np.isnan(df[factor]), factor] 
                                        for factor in df.columns.values
                                ]
                                )
                            ttest_stats = pg.ttest(df[df.columns[0]], df[df.columns[1]], paired=True)
                            statistic = ttest_stats['T'].values[0]
                            pvalue = ttest_stats['p-val'].values[0]
                            effsize = ttest_stats['cohen-d'].values[0]
                            statDf = statDf.append(
                            {'Comparison': sheetDf.columns.values[i] + '|' + sheetDf.columns.values[j],
                            'N': sheetDf[sheetDf.columns.values[i]].size,
                            'Normality':allColumnsAreNormal,
                            'Test': "Student",
                            'Statistic':statistic,
                            'pValue':pvalue,
                            'effectSize':effsize
                            }, ignore_index=True)
                        except ValueError as StudentError:
                            statDf = statDf.append(
                            {'Comparison': sheetDf.columns.values[i] + '|' + sheetDf.columns.values[j],
                            'N': sheetDf[sheetDf.columns.values[i]].size,
                            'Normality':allColumnsAreNormal,
                            'Test': "Student",
                            'Statistic':-1,
                            'pValue':-1,
                            'effectSize':-1
                            }, ignore_index=True)
            reject, statDf.loc[uncorrectedStatIndex::,'pValue'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'pValue'].values, alpha=0.05, method="bonf")
        
        else :
             # ALL MODALITY
            if len(sheetDf.columns) > 2:
                sheetDf_long = sheetDf.melt(ignore_index=False).reset_index()
                friedman_stats = pg.friedman(data=sheetDf_long, dv="value", within="variable", subject="index")
                source, wvalue, ddof1, qvalue, pvalue = friedman_stats.values[0]
                statDf = statDf.append(
                    {'Comparison': sheetDf.columns.str.cat(sep=' | '),
                    'N': sheetDf[sheetDf.columns.values[0]].size,
                    'Normality':allColumnsAreNormal,
                    'Test': "Friedman",
                    'Statistic':qvalue,
                    'pValue':pvalue,
                    'effectSize':wvalue
                    }, ignore_index=True)

            # BETWEEN MODALITY
            modality_names = sheetDf.columns.values
            uncorrectedStatIndex = len(statDf.index)
            for i in range(len(modality_names)):
                for j in range(i+1, len(modality_names)):
                    if Statistics.areRelatedFactors(modality_names[i], modality_names[j]):
                        x = sheetDf.loc[:, modality_names[i]]
                        y =  sheetDf.loc[:, modality_names[j]]
                        stats_wilcoxon = pg.wilcoxon(x,y, alternative='two-sided')
                        wvalue, alternative, pvalue, rbc, CLES = stats_wilcoxon.values[0]
                        statDf = statDf.append(
                                {
                                    'Comparison': modality_names[i] + '|' + modality_names[j],
                                    'N': sheetDf[sheetDf.columns.values[i]].size,
                                    'Normality':allColumnsAreNormal,
                                    'Test': "Wilcoxon",
                                    'Statistic':wvalue,
                                    'pValue':pvalue,
                                    'effectSize': rbc
                                }, ignore_index=True)
                        
            uncorrected_pvalues = np.array(statDf.loc[uncorrectedStatIndex::,'pValue'].values, dtype=np.float64)
            reject, statDf.loc[uncorrectedStatIndex::,'pValue'] = pg.multicomp(uncorrected_pvalues, alpha=0.05, method="bonf")
        return statDf


    # @staticmethod
    # def qualNominalPaired(imgDir, sheetName, sheetDf):
    #     print("######################################## ",sheetName," ########################################")
    #     meltedSheetDf = sheetDf.melt(var_name='columns', value_name='index')
    #     contingencySheetTable = pd.crosstab(index=meltedSheetDf['index'], columns=meltedSheetDf['columns'])
    #     #if len(splittedSheetName) > 1:
    #     #     orderedColumns = splittedSheetName[1].split('>')
    #     #     contingencySheetTable = contingencySheetTable.reindex(orderedColumns)
    #     contingencySheetTable.loc['COUNT'] = contingencySheetTable.sum().values
    #     contingencySheetTable = contingencySheetTable.drop(index='COUNT')
        
    #     if len(sheetDf.columns) > 2:
    #         print(contingencySheetTable)
    #         chi2, pvalue, dof, ex = stats.chi2_contingency(contingencySheetTable.T)
    #         print( sheetDf.columns.str.cat(sep=' | '), " -> CHI² (statistic: ", chi2, ", p-value: ", pvalue,")")
    #     for i in range(len(sheetDf.columns.values)):
    #         for j in range(i+1, len(sheetDf.columns.values)):
    #             try:
    #                 ctab = contingencySheetTable[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
    #                 print(ctab)
    #                 chi2, pvalue, dof, ex = stats.chi2_contingency(ctab)
    #                 print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> CHI² (statistic: ", chi2, ", p-value: ", pvalue,")")
    #             except ValueError as ChiError:
    #                 print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> CHI² (",ChiError,")")
    #                 try:
    #                     ctab = contingencySheetTable[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
    #                     oddsratio, pvalue = stats.fisher_exact(ctab)
    #                     print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j], " -> Fisher (statistic: ", oddsratio, ", p-value: ", pvalue,")")
    #                 except ValueError as FisherError:
    #                     print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j], " -> Fisher (", FisherError,")")
    
    # @staticmethod
    # def qualNominalUnpaired(imgDir,  sheetName, sheetDf):
    #     print("######################################## ",sheetName," ########################################")
    #     meltedSheetDf = sheetDf.melt(var_name='columns', value_name='index')
    #     contingencySheetTable = pd.crosstab(index=meltedSheetDf['index'], columns=meltedSheetDf['columns'])
    #     #if len(splittedSheetName) > 1:
    #     #    orderedColumns = splittedSheetName[1].split('>')
    #     #    contingencySheetTable = contingencySheetTable.reindex(orderedColumns)
    #     contingencySheetTable.loc['COUNT'] = contingencySheetTable.sum().values
        
    #     if len(sheetDf.columns) > 2:
    #         print(contingencySheetTable)
    #         contingencySheetTable = contingencySheetTable.drop(index='COUNT')
    #         chi2, pvalue, dof, ex = stats.chi2_contingency(contingencySheetTable.T)
    #         print( sheetDf.columns.str.cat(sep=' | '), " -> CHI² (statistic: ", chi2, ", p-value: ", pvalue,")")
        
    #     for i in range(len(sheetDf.columns.values)):
    #         for j in range(i+1, len(sheetDf.columns.values)):
    #             try:
    #                 ctab = contingencySheetTable[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
    #                 print(ctab)
    #                 chi2, pvalue, dof, ex = stats.chi2_contingency(ctab)
    #                 print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> CHI² (statistic: ", chi2, ", p-value: ", pvalue,")")
    #             except ValueError as ChiError:
    #                 print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> CHI² (",ChiError,")")
    #                 try:
    #                     ctab = contingencySheetTable[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
    #                     oddsratio, pvalue = stats.fisher_exact(ctab)
    #                     print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j], " -> Fisher (statistic: ", oddsratio, ", p-value: ", pvalue,")")
    #                 except ValueError as FisherError:
    #                     print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j], " -> Fisher (", FisherError,")")
    #     StackedBarPlotter.StackedBarPlotter(
    #      filename =  imgDir + '/' + sheetName + '.png', 
    #      title = sheetName,
    #      sheetDf = contingencySheetTable)

    @staticmethod
    def qualOrdinalPaired(imgDir,  sheetName, sheetDf, sheetScale, silent=True):
        print("######################################## ",sheetName," ########################################") if not silent else None
        print(Statistics.describePlus(sheetDf)) if not silent else None
        meltedSheetDf = sheetDf.melt(var_name='Factor', value_name='variable')
        contingencySheetDf = pd.crosstab(index=meltedSheetDf['variable'], columns=meltedSheetDf['Factor'])
        statDf = pd.DataFrame(columns=['COMPARISON', 'TEST', 'STATISTICS', 'P-VALUE', 'EFFECT SIZE'])
        #fill empty scale value
        for sheetStep in range(sheetScale):
            if not sheetStep in contingencySheetDf.index.values:
                contingencySheetDf.loc[sheetStep] = [0 for x in range(len(contingencySheetDf.columns.values))]
        contingencySheetDf.sort_index(inplace=True)

        # ALL MODALITY
        if len(contingencySheetDf.columns) > 2:
            sheetDf_long = sheetDf.melt(ignore_index=False).reset_index()
            friedman_stats = pg.friedman(data=sheetDf_long, dv="value", within="variable", subject="index")
            source, wvalue, ddof1, qvalue, pvalue = friedman_stats.values[0]
            statDf = statDf.append(
                {'COMPARISON': 'ALL',
                'TEST': "Friedman",
                'STATISTICS':qvalue,
                'P-VALUE':pvalue,
                'EFFECT SIZE':wvalue
                }, ignore_index=True)
            print(sheetDf.columns.str.cat(sep=' | '), " -> Friedman (statistic:", qvalue, " p-value: ", pvalue, ", effect size:", wvalue, ")") if not silent else None
                
        # BETWEEN MODALITY
        modality_names = sheetDf.columns.values
        uncorrectedStatIndex = len(statDf.index)
        for i in range(len(modality_names)):
            for j in range(i+1, len(modality_names)):
                    stats_wilcoxon = pg.wilcoxon(sheetDf.loc[:, modality_names[i]], sheetDf.loc[:, modality_names[j]],  alternative='two-sided')
                    wvalue, alternative, pvalue, RBC, CLES = stats_wilcoxon.values[0]
                    statDf = statDf.append(
                        {
                            'COMPARISON': modality_names[i] + '|' + modality_names[j],
                            'TEST': "Wilcoxon",
                            'STATISTICS':wvalue,
                            'P-VALUE':pvalue,
                            'EFFECT SIZE': RBC
                        }, ignore_index=True)
        statDf =  statDf.infer_objects()
        
        uncorrected_pvalues = np.array(statDf.loc[uncorrectedStatIndex::,'P-VALUE'].values, dtype=np.float64)
        reject, statDf.loc[uncorrectedStatIndex::,'P-VALUE'] = pg.multicomp(uncorrected_pvalues, alpha=0.05, method="bonf")
        for i in range(len(modality_names)):
            for j in range(i+1, len(modality_names)):
                    stats = statDf.loc[statDf['COMPARISON'] == sheetDf.columns.values[i] + '|' +  sheetDf.columns.values[j], :]
                    print(stats['COMPARISON'].values[0], " -> Wilcoxon (statistic:", stats['STATISTICS'].values[0], " p-value: ", stats['P-VALUE'].values[0], ", effectsize:", stats['EFFECT SIZE'].values[0], ")") if not silent else None
                
        StackedBarPlotter.StackedBarPlotter(
         filename =  imgDir + '/' + sheetName + '.png', 
         title = sheetName,
         dataDf = sheetDf,
         histDf = contingencySheetDf,
         statDf = statDf)

    # @staticmethod
    # def qualOrdinalUnpaired(imgDir, sheetName, sheetDf, sheetScale, silent=False):
    #     print("######################################## ",sheetName," ########################################") if not silent else None
    #     print(Statistics.describePlus(sheetDf)) if not silent else None
    #     meltedSheetDf = sheetDf.melt(var_name='Factor', value_name='variable')
    #     contingencySheetDf = pd.crosstab(index=meltedSheetDf['variable'], columns=meltedSheetDf['Factor'])
    #     statDf = pd.DataFrame(columns=['COMPARISON', 'TEST', 'STATISTICS', 'P-VALUE', 'EFFECT SIZE'])
    #     #fill empty scale value
    #     for sheetStep in range(sheetScale):
    #         if not sheetStep in contingencySheetDf.index.values:
    #             contingencySheetDf.loc[sheetStep] = [0 for x in range(len(contingencySheetDf.columns.values))]
    #     contingencySheetDf.sort_index(inplace=True)
    #     # ALL MODALITY
    #     if len(contingencySheetDf.columns) > 2:
    #         sheetDf_long = sheetDf.melt(ignore_index=False).reset_index()
    #         kruskal_stats = pg.kruskal(data=sheetDf_long, dv="value", between="variable")
    #         source, ddof1, hvalue, pvalue = kruskal_stats.values[0]
    #         statDf = statDf.append(
    #             {'COMPARISON': 'ALL',
    #             'TEST': "Kruskal-Wallis",
    #             'STATISTICS':hvalue,
    #             'P-VALUE':pvalue,
    #             'EFFECT SIZE':-1
    #             }, ignore_index=True)

    #     # BETWEEN MODALITY
    #     modality_names = sheetDf.columns.values
    #     uncorrectedStatIndex = len(statDf.index)
    #     for i in range(len(modality_names)):
    #         for j in range(i+1, len(modality_names)):
    #                 stats_mannwhitney = pg.mwu(x=sheetDf.loc[:, modality_names[i]], y=sheetDf.loc[:, modality_names[j]], alternative='two-sided')
    #                 uvalue, alternative, pvalue, RBC, CLES = stats_mannwhitney.values[0]
    #                 statDf = statDf.append(
    #                     {
    #                         'COMPARISON': modality_names[i] + '|' + modality_names[j],
    #                         'TEST': "Mann-Whitney",
    #                         'STATISTICS':uvalue,
    #                         'P-VALUE':pvalue,
    #                         'EFFECT SIZE': RBC
    #                     }, ignore_index=True)
    #     reject, statDf.loc[uncorrectedStatIndex::,'P-VALUE'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'P-VALUE'].values, alpha=0.05, method="bonf")

    #     StackedBarPlotter.StackedBarPlotter(
    #      filename =  imgDir + '/' + sheetName + '.png', 
    #      title = sheetName,
    #      dataDf = sheetDf,
    #      histDf = contingencySheetDf,
    #      statDf = statDf)

    # @staticmethod
    # def qualOrdinalPaired(sheetDf, sheetScale) -> pd.DataFrame:
    #     meltedSheetDf = sheetDf.melt(var_name='Factor', value_name='variable')
    #     contingencySheetDf = pd.crosstab(index=meltedSheetDf['variable'], columns=meltedSheetDf['factor'])
    #     statDf = pd.DataFrame(columns=['Comparison', 'N', 'Test', 'Statistic', 'pValue', 'effectSize'])
    #     #fill empty scale value
    #     for sheetStep in range(sheetScale):
    #         if not sheetStep in contingencySheetDf.index.values:
    #             contingencySheetDf.loc[sheetStep] = [0 for x in range(len(contingencySheetDf.columns.values))]
    #     contingencySheetDf.sort_index(inplace=True)

    #     # ALL MODALITY
    #     if len(contingencySheetDf.columns) > 2:
    #         sheetDf_long = sheetDf.melt(ignore_index=False).reset_index()
    #         friedman_stats = pg.friedman(data=sheetDf_long, dv="value", within="variable", subject="index")
    #         source, wvalue, ddof1, qvalue, pvalue = friedman_stats.values[0]
    #         statDf = statDf.append(
    #             {'Comparison': sheetDf.columns.str.cat(sep=' | '),
    #             'N': sheetDf[sheetDf.columns.values[0]].size,
    #             'Test': "Friedman",
    #             'Statistic':qvalue,
    #             'pValue':pvalue,
    #             'effectSize':wvalue
    #             }, ignore_index=True)

    #     # BETWEEN MODALITY
    #     modality_names = sheetDf.columns.values
    #     uncorrectedStatIndex = len(statDf.index)
    #     for i in range(len(modality_names)):
    #         for j in range(i+1, len(modality_names)):
    #             if Statistics.areRelatedfactors(modality_names[i], modality_names[j]):
    #                 stats_wilcoxon = pg.wilcoxon(sheetDf.loc[:, modality_names[i]], sheetDf.loc[:, modality_names[j]],  alternative='two-sided')
    #                 wvalue, alternative, pvalue, RBC, CLES = stats_wilcoxon.values[0]
    #                 statDf = statDf.append(
    #                     {
    #                         'Comparison': modality_names[i] + '|' + modality_names[j],
    #                         'N': sheetDf[sheetDf.columns.values[i]].size,
    #                         'Test': "Wilcoxon",
    #                         'Statistic':wvalue,
    #                         'pValue':pvalue,
    #                         'effectSize': RBC
    #                     }, ignore_index=True)
    #     reject, statDf.loc[uncorrectedStatIndex::,'pValue'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'pValue'].values, alpha=0.05, method="bonf")
    #     return statDf
        
    # @staticmethod
    # def inferQuantPaired(sheetDf) -> pd.DataFrame:
    #     normalityDf = Statistics.normalityAsumption(sheetDf)
    #     allColumnsAreNormal = normalityDf.loc['norm'].sum() == len(normalityDf.columns.values)
    #     statDf = pd.DataFrame(columns=['Comparison', 'N', 'Normality', 'Test', 'Statistic', 'pValue', 'effectSize'])
    #     if allColumnsAreNormal:
    #         if len(sheetDf.columns) > 2:
    #             aov = pg.rm_anova(sheetDf) # two-ways repeated-measures ANOVA
    #             statistic = aov['F'].values[0]
    #             pvalue = aov['p-GG-corr'].values[0] if 'p-GG-corr' in aov.columns.values else aov['p-unc'].values[0]
    #             effsize = aov['np2'].values[0]
    #             statDf = statDf.append(
    #                 {'Comparison':  sheetDf.columns.str.cat(sep=' | '),
    #                 'N': sheetDf[sheetDf.columns.values[0]].size,
    #                 'Normality':allColumnsAreNormal,
    #                 'Test': "ANOVA",
    #                 'Statistic':statistic,
    #                 'pValue':pvalue,
    #                 'effectSize':effsize
    #                 }, ignore_index=True)
            
    #         uncorrectedStatIndex = len(statDf.index)
    #         for i in range(len(sheetDf.columns.values)):
    #             for j in range(i+1, len(sheetDf.columns.values)):
    #                 if Statistics.areRelatedfactors(sheetDf.columns.values[i], sheetDf.columns.values[j]):
    #                     try:
    #                         df = sheetDf[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
    #                         statistic, pvalue = stats.ttest_ind(
    #                             *[
    #                                     df.loc[~np.isnan(df[factor]), factor] 
    #                                     for factor in df.columns.values
    #                             ]
    #                             )
    #                         ttest_stats = pg.ttest(df[df.columns[0]], df[df.columns[1]], paired=True)
    #                         statistic = ttest_stats['T'].values[0]
    #                         pvalue = ttest_stats['p-val'].values[0]
    #                         effsize = ttest_stats['cohen-d'].values[0]
    #                         statDf = statDf.append(
    #                         {'Comparison': sheetDf.columns.values[i] + '|' + sheetDf.columns.values[j],
    #                         'N': sheetDf[sheetDf.columns.values[i]].size,
    #                         'Normality':allColumnsAreNormal,
    #                         'Test': "Student",
    #                         'Statistic':statistic,
    #                         'pValue':pvalue,
    #                         'effectSize':effsize
    #                         }, ignore_index=True)
    #                     except ValueError as StudentError:
    #                         statDf = statDf.append(
    #                         {'Comparison': sheetDf.columns.values[i] + '|' + sheetDf.columns.values[j],
    #                         'N': sheetDf[sheetDf.columns.values[i]].size,
    #                         'Normality':allColumnsAreNormal,
    #                         'Test': "Student",
    #                         'Statistic':-1,
    #                         'pValue':-1,
    #                         'effectSize':-1
    #                         }, ignore_index=True)
    #         reject, statDf.loc[uncorrectedStatIndex::,'pValue'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'pValue'].values, alpha=0.05, method="bonf")
        
    #     else :
    #          # ALL MODALITY
    #         if len(sheetDf.columns) > 2:
    #             sheetDf_long = sheetDf.melt(ignore_index=False).reset_index()
    #             friedman_stats = pg.friedman(data=sheetDf_long, dv="value", within="variable", subject="index")
    #             source, wvalue, ddof1, qvalue, pvalue = friedman_stats.values[0]
    #             statDf = statDf.append(
    #                 {'Comparison': sheetDf.columns.str.cat(sep=' | '),
    #                 'N': sheetDf[sheetDf.columns.values[0]].size,
    #                 'Normality':allColumnsAreNormal,
    #                 'Test': "Friedman",
    #                 'Statistic':qvalue,
    #                 'pValue':pvalue,
    #                 'effectSize':wvalue
    #                 }, ignore_index=True)

    #         # BETWEEN MODALITY
    #         modality_names = sheetDf.columns.values
    #         uncorrectedStatIndex = len(statDf.index)
    #         for i in range(len(modality_names)):
    #             for j in range(i+1, len(modality_names)):
    #                 if Statistics.areRelatedfactors(modality_names[i], modality_names[j]):
    #                     x = sheetDf.loc[:, modality_names[i]]
    #                     y =  sheetDf.loc[:, modality_names[j]]
    #                     stats_wilcoxon = pg.wilcoxon(x,y, alternative='two-sided')
    #                     wvalue, alternative, pvalue, rbc, CLES = stats_wilcoxon.values[0]
    #                     statDf = statDf.append(
    #                             {
    #                                 'Comparison': modality_names[i] + '|' + modality_names[j],
    #                                 'N': sheetDf[sheetDf.columns.values[i]].size,
    #                                 'Normality':allColumnsAreNormal,
    #                                 'Test': "Wilcoxon",
    #                                 'Statistic':wvalue,
    #                                 'pValue':pvalue,
    #                                 'effectSize': rbc
    #                             }, ignore_index=True)
    #         reject, statDf.loc[uncorrectedStatIndex::,'pValue'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'pValue'].values, alpha=0.05, method="bonf")
    #     return statDf

    # @staticmethod
    # def quantPaired(imgDir, sheetName, sheetDf, showDf=False, silent=True):
    #     print("######################################## ",sheetName," ########################################") if not silent else None
    #     print(Statistics.describePlus(sheetDf)) if not silent else None
    #     normalityDf = Statistics.normalityAsumption(sheetDf)
    #     allColumnsAreNormal = normalityDf.loc['norm'].sum()
    #     print(normalityDf) if not silent else None
    #     statDf = pd.DataFrame(columns=['COMPARISON', 'TEST', 'STATISTICS', 'P-VALUE', 'EFFECT SIZE'])
    #     if allColumnsAreNormal == len(normalityDf.columns.values):
    #         print("Normality assumed")
    #         if len(sheetDf.columns) > 2:
    #             print(sheetDf) if showDf else None
    #             aov = pg.rm_anova(sheetDf) # two-ways repeated-measures ANOVA
    #             statistic = aov['F'].values[0]
    #             pvalue = aov['p-GG-corr'].values[0] if 'p-GG-corr' in aov.columns.values else aov['p-unc'].values[0]
    #             effsize = aov['np2'].values[0]
    #             print( sheetDf.columns.str.cat(sep=' | '), " -> ANOVA (statistic:", statistic, " p-value: ", pvalue, ")") if not silent else None
    #             statDf = statDf.append(
    #                 {'COMPARISON': 'ALL',
    #                 'TEST': "ANOVA",
    #                 'STATISTICS':statistic,
    #                 'P-VALUE':pvalue,
    #                 'EFFECT SIZE':effsize
    #                 }, ignore_index=True)
    #         for i in range(len(sheetDf.columns.values)):
    #             for j in range(i+1, len(sheetDf.columns.values)):
    #                 try:
    #                     df = sheetDf[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
    #                     print(df) if showDf else None
    #                     statistic, pvalue = stats.ttest_ind(
    #                         *[
    #                                 df.loc[~np.isnan(df[factor]), factor] 
    #                                 for factor in df.columns.values
    #                         ]
    #                         )
    #                     ttest_stats = pg.ttest(df[df.columns[0]], df[df.columns[1]], paired=True)
    #                     statistic = ttest_stats['T'].values[0]
    #                     pvalue = ttest_stats['p-val'].values[0]
    #                     effsize = ttest_stats['cohen-d'].values[0]
    #                     print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> Student (statistic: ", statistic, ", p-value: ", pvalue,")")  if not silent else None
    #                     statDf = statDf.append(
    #                     {'COMPARISON': sheetDf.columns.values[i] + '|' + sheetDf.columns.values[j],
    #                     'TEST': "Student",
    #                     'STATISTICS':statistic,
    #                     'P-VALUE':pvalue,
    #                     'EFFECT SIZE':effsize
    #                     }, ignore_index=True)
    #                 except ValueError as StudentError:
    #                     print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> Student (",StudentError,")") if not silent else None
    #                     statDf = statDf.append(
    #                     {'COMPARISON': sheetDf.columns.values[i] + '|' + sheetDf.columns.values[j],
    #                     'TEST': "Student",
    #                     'STATISTICS':-1,
    #                     'P-VALUE':-1,
    #                     'EFFECT SIZE':-1
    #                     }, ignore_index=True)
    #     else :
    #         print("Normality not assumed")
    #          # ALL MODALITY
    #         if len(sheetDf.columns) > 2:
    #             sheetDf_long = sheetDf.melt(ignore_index=False).reset_index()
    #             friedman_stats = pg.friedman(data=sheetDf_long, dv="value", within="variable", subject="index")
    #             source, wvalue, ddof1, qvalue, pvalue = friedman_stats.values[0]
    #             statDf = statDf.append(
    #                 {'COMPARISON': 'ALL',
    #                 'TEST': "Friedman",
    #                 'STATISTICS':qvalue,
    #                 'P-VALUE':pvalue,
    #                 'EFFECT SIZE':wvalue
    #                 }, ignore_index=True)
    #             print(sheetDf.columns.str.cat(sep=' | '), " -> Friedman (statistic:", qvalue, " p-value: ", pvalue, ", effect size:", wvalue, ")") if not silent else None
                

    #         # BETWEEN MODALITY
    #         modality_names = sheetDf.columns.values
    #         uncorrectedStatIndex = len(statDf.index)
    #         for i in range(len(modality_names)):
    #             for j in range(i+1, len(modality_names)):
    #                 x = sheetDf.loc[:, modality_names[i]]
    #                 y =  sheetDf.loc[:, modality_names[j]]
    #                 stats_wilcoxon = pg.wilcoxon(x,y, alternative='two-sided')
    #                 wvalue, alternative, pvalue, rbc, CLES = stats_wilcoxon.values[0]
    #                 statDf = statDf.append(
    #                         {
    #                             'COMPARISON': modality_names[i] + '|' + modality_names[j],
    #                             'TEST': "Wilcoxon",
    #                             'STATISTICS':wvalue,
    #                             'P-VALUE':pvalue,
    #                             'EFFECT SIZE': rbc
    #                         }, ignore_index=True)
    #         reject, statDf.loc[uncorrectedStatIndex::,'P-VALUE'] = pg.multicomp(statDf.loc[uncorrectedStatIndex::,'P-VALUE'].values, alpha=0.05, method="bonf")
    #         for i in range(len(modality_names)):
    #             for j in range(i+1, len(modality_names)):
    #                 stats = statDf.loc[statDf['COMPARISON'] == sheetDf.columns.values[i] + '|' +  sheetDf.columns.values[j], :]
    #                 print(stats['COMPARISON'].values[0], " -> Wilcoxon (statistic:", stats['STATISTICS'].values[0], " p-value: ", stats['P-VALUE'].values[0], ", effectsize:", stats['EFFECT SIZE'].values[0], ")") if not silent else None
                
    #     BoxPlotter.BoxPlotter(
    #     filename =  imgDir + '/' + sheetName + '.png', 
    #     title = sheetName,
    #     sheetDf = sheetDf,
    #     statDf = statDf)

        

    # @staticmethod
    # def quantUnpaired(imgDir, sheetName, sheetDf, showDf=False, silent=True):
    #     print("######################################## ",sheetName," ########################################") if not silent else None
    #     print(Statistics.describePlus(sheetDf)) if not silent else None
    #     normalityDf = Statistics.normalityAsumption(sheetDf)
    #     print(normalityDf) if not silent else None
    #     statDf = pd.DataFrame(columns=['COMPARISON', 'TEST', 'STATISTICS', 'P-VALUE', 'EFFECT SIZE'])
    #     if len(sheetDf.columns) > 2:
    #         print(sheetDf) if showDf else None
    #         aov = pg.rm_anova(sheetDf)
    #         statistic = aov['F'].values[0]
    #         pvalue = aov['p-GG-corr'].values[0] if 'p-GG-corr' in aov.columns.values else aov['p-unc'].values[0]
    #         effsize = aov['np2'].values[0]
    #         print( sheetDf.columns.str.cat(sep=' | '), " -> ANOVA (statistic:", statistic, " p-value: ", pvalue, ")") if not silent else None
    #         statDf = statDf.append(
    #             {'COMPARISON': 'ALL',
    #             'TEST': "ANOVA",
    #             'STATISTICS':statistic,
    #             'P-VALUE':pvalue,
    #             'EFFECT SIZE':effsize
    #             }, ignore_index=True)
    #     for i in range(len(sheetDf.columns.values)):
    #         for j in range(i+1, len(sheetDf.columns.values)):
    #             try:
    #                 df = sheetDf[[sheetDf.columns.values[i], sheetDf.columns.values[j]]]
    #                 print(df) if showDf else None
    #                 statistic, pvalue = stats.ttest_ind(
    #                     *[
    #                             df.loc[~np.isnan(df[factor]), factor] 
    #                             for factor in df.columns.values
    #                     ]
    #                     )
    #                 ttest_stats = pg.ttest(df[df.columns[0]], df[df.columns[1]], paired=False)
    #                 statistic = ttest_stats['T'].values[0]
    #                 pvalue = ttest_stats['p-val'].values[0]
    #                 effsize = ttest_stats['cohen-d'].values[0]
    #                 print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> Student (statistic: ", statistic, ", p-value: ", pvalue,")")  if not silent else None
    #                 statDf = statDf.append(
    #                 {'COMPARISON': sheetDf.columns.values[i] + '|' + sheetDf.columns.values[j],
    #                 'TEST': "Student",
    #                 'STATISTICS':statistic,
    #                 'P-VALUE':pvalue,
    #                 'EFFECT SIZE':effsize
    #                 }, ignore_index=True)
    #             except ValueError as StudentError:
    #                 print(sheetDf.columns.values[i],'|', sheetDf.columns.values[j],  " -> Student (",StudentError,")") if not silent else None
    #                 statDf = statDf.append(
    #                 {'COMPARISON': sheetDf.columns.values[i] + '|' + sheetDf.columns.values[j],
    #                 'TEST': "Student",
    #                 'STATISTICS':-1,
    #                 'P-VALUE':-1,
    #                 'EFFECT SIZE':-1
    #                 }, ignore_index=True)
    #     BoxPlotter.BoxPlotter(
    #      filename =  imgDir + '/' + sheetName + '.png', 
    #      title = sheetName,
    #      sheetDf = sheetDf,
    #      statDf = statDf)
         
    # @staticmethod
    # def biQuantUnpaired(imgDir, sheetName, sheetDf, describeDf=False):
    #     print("######################################## ",sheetName," ########################################")
    #     print(Statistics.describePlus(sheetDf)) if describeDf else None
    #     try:
    #         print(sheetDf)
    #         coefficient, pvalue = stats.pearsonr(sheetDf.iloc[:,0], sheetDf.iloc[:,1])
    #         print(sheetDf.columns.values[0],'|', sheetDf.columns.values[1], " -> Pearson Correlation (coefficient: ", coefficient,", pvalue: ", pvalue, ")")
    #         try:
    #             slope, intercept, r_value, p_value, std_err = stats.linregress(sheetDf.iloc[:,0], sheetDf.iloc[:,1])
    #             print(sheetDf.columns.values[0],'|', sheetDf.columns.values[1], " -> Linear Regression (slope: ",slope, ", intercept: ", intercept, ", r_value: ", r_value, ", p_value: ", p_value, ", std_err: ", std_err, ")")
    #         except ValueError as LinearRegressError:
    #             print(sheetDf.columns.values[0],'|', sheetDf.columns.values[1], " -> Linear Regression (",LinearRegressError,")")
    #     except ValueError as PearsonError:
    #         print(sheetDf.columns.values[0],'|', sheetDf.columns.values[1], " -> Pearson Correlation (",PearsonError,")")
       
    #     ScatterPlotter.ScatterPlotter(
    #      filename =  imgDir + '/' + sheetName + '.png', 
    #      title = sheetName,
    #      sheetDf = sheetDf,
    #      slope= slope,
    #      intercept= intercept)

         
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


