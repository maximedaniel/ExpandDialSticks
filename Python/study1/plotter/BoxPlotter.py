import numpy as np
import pandas as pd
from scipy import stats
import time
import matplotlib.pyplot as pyplot
from colour import Color
import seaborn as sns

class BoxPlotter:
     def __init__(self, filename, title, sheetDf):
        df = sheetDf
        print(df)
        height=0.75
        fig = pyplot.figure(1, figsize=(len(df.columns.values)+1, 1))
        ax = fig.add_subplot(1,1,1) #, aspect=1
        ax.spines['right'].set_visible(False)
        ax.spines['top'].set_visible(False)
        pyplot.title(r"$\bf{" + str(title) + "}$", size=14, y=1.15)
        colorRange = sns.color_palette("RdYlGn", 7)
        columnIndexes = np.arange(len(df.columns.values))    # the x locations for the groups

        pyplot.boxplot(
          x=df.values,
          notch=False,
          vert=False,
          positions=columnIndexes,
          widths=[ height for columnValue in df.columns.values],
          #patch_artist=True,
          #boxprops=dict(facecolor=colorRange[3], color=colorRange[-1]),
          #capprops=dict(color=colorRange[-1]),
          #whiskerprops=dict(color=colorRange[-1]),
          #flierprops=dict(color=colorRange[0], markeredgecolor=colorRange[0]),
          #medianprops=dict(color=colorRange[0])
        )
        pyplot.xlabel('MEASURE', size=9)
        ax.tick_params(axis='both', which='major', labelsize=9)
        pyplot.yticks(columnIndexes, df.columns.values)
        #ax.set_xlim([-100, 100])
        #vals = ax.get_xticks()
        #ax.set_xticklabels([ str(int(abs(x))) + '%' for x in vals])

        fig.savefig(filename, bbox_inches='tight', dpi = 300)
        pyplot.close("all")
