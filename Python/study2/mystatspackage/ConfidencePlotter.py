import numpy as np
import pandas as pd
from scipy import stats
import time
import matplotlib.pyplot as pyplot
from matplotlib.font_manager import FontProperties
from colour import Color
import seaborn as sns
import pingouin as pg

MAX_STAT_ROWS = 10
class ConfidencePlotter:
     def __init__(self, filename, title, sheetDf, yAxisLabels, statDf=None):
       
        fig, ax = pyplot.subplots()
        #ax.plot(x, y, "k")
        #ax.set(aspect=1)
        height=0.75
        # Figure 1
        columnIndexes = np.arange(len(sheetDf.columns.values))    # the x locations for the groups
        
        # Figure 2
        for i in columnIndexes:
          columnName = sheetDf.columns.values[i]
          # Compute MEAN and CI
          column_mean = sheetDf[columnName].mean()
          column_ci = pg.compute_bootci(sheetDf.loc[~np.isnan(sheetDf[columnName]), columnName],func='mean')
          # print('%s -> %f[%f %f](95 CI)' %(columnName, column_mean, column_ci[0], column_ci[1]))
          ax.plot(column_mean, i, 'o', markersize=6, markerfacecolor='white', markeredgecolor='black', markeredgewidth=1.5) 
          ax.hlines(columnName, column_ci[0], column_ci[1], colors='black', linestyles='solid', linewidth=1.5)
          ax.barh(y=columnName, width=0.1, height=height, left=0, color='white')
        #print(histDf.index.asttypvalues.tolist())
        #print(dataDf.index.values)
        ax.set_xlabel('Score', size=9)
        ax.set_yticks(columnIndexes)
        ax.set_yticklabels(['%s' %column_name for column_name in sheetDf.columns.values])
        # axs[1].set_xticks(histDf.index.astype(float).values)

        ax.spines['right'].set_visible(False)
        ax.spines['top'].set_visible(False)

        ax.tick_params(axis='y', labelsize=9)
        ax.tick_params(axis='x', labelsize=7)
        pyplot.show()
        print(filename)
        fig.savefig(filename, bbox_inches='tight', dpi = 300)
        pyplot.close("all")
