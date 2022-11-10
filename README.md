# twn
Tween libs with special effort put toward simplicity, conciseness, upfront memory allocation, and low overhead.

## Usage
You can use either single/double file entries. If you intend on using multiple tween libraries, I would suggest using the double file entry and only keeping one utl file. This way, multiple twn classes can leverage those structures/variables.

<img src="https://github.com/rchanlatte95/twn/blob/main/README_GIFs/unity_showcase.mov" width="640" height="360" />

### Single File Libraries
You can choose single files that contain all the code you need to run the tween library of choice. These are self contained and have less than 500 sloc per file. They have custom structures, enums, and constants to aid in their execution.

### Double File Libraries
You can choose two files that contain all the code you need to run the tween library of choice. One of the files extracts common structures and constants between each single file library to condense things. The second file is the same as the single file libraries without the structure or constant definitions.

## Performance compared to other tween libraries
(TODO_RYAN_2022-11-10)

## Memory footprint compared to other tween libraries
(TODO_RYAN_2022-11-10)
