# Instructions #

GRIDDA performs two key functions, shapefile delineation and gridded data extraction.

## Modes ##
|-ALL|Perform both delineation and extraction|
|:---|:--------------------------------------|
|-DELINEATE|Only perform shapefile delineation|
|-EXTRACT|Only perform data extraction|
|-GUI|Graphical user interface|

## Arguments ##

### ALL ###
ALL mode requires the combined arguments of **-DELINEATE** and **-EXTRACT**, excluding **-WEIGHTFILE**.

_**Example**_
GRIDDA.exe -ALL -SHAPEFILE=orara\_river.shp -OUTDIR=AllTest`\` -GRID=-43.575,112.925,0.05,0.05,813,670 -DATADIR=Data`\` -TIMEUNIT=mo


### DELINEATE ###
| -SHAPEFILE=`<path>` | Path to shapefile |
|:--------------------|:------------------|
| -OUTDIR=`<path>` | Directory to place output |
| -GRID=a,b,c,d,e,f | Where a and b are longitude and latitude of the lower left corner of the grid, c and d are cell width and height, e and f are number of columns and rows. |

_**Example**_
GRIDDA.exe -DELINEATE -SHAPEFILE=orara\_river.shp -OUTDIR=DelineateTest`\` -GRID=-43.575,112.925,0.05,0.05,813,670


### EXTRACT ###
| -WEIGHTFILE=`<path>` | File containing cell weights |
|:---------------------|:-----------------------------|
| -DATADIR=`<path>` | File containing gridded data |
| -TIMEUNIT=<hr|da|mo> | Time unit of gridded data |
| -OUTDIR=`<path>` | Directory to place output |
| -GRID=a,b,c,d,e,f | Where a and b are longitude and latitude of the lower left corner of the grid, c and d are cell width and height, e and f are number of columns and rows. |

_**Example**_
GRIDDA.exe -EXTRACT -WEIGHTFILE=DelineateTest\Weights.csv -OUTDIR=ExtractTest`\` -GRID=-43.575,112.925,0.05,0.05,813,670 -DATADIR=Data`\` -TIMEUNIT=mo


### Optional Arguments ###
| -BOUNDARY | Produce images of catchment boundaries |
|:----------|:---------------------------------------|
| -AREA | Produce images of catchment area |
| -STATS | Produce timeseries statistics |
| -PLOTTIME | Produce timeseries plots |
| -PLOTSTAT | Produce timeseries statistic plots |