# FlashfloodSegmentation
This is an image segmentation program (similar to the watershed algorithm) that was written independently as a personal project/learning exercise
(Freshman Year @ UPenn, Fall 2017 semester).

The image is broken up into tiny sections based on a color similarity threshold, and those sections are then recombined (by analyzing edge similarity)
until the segments are sufficiently large to be meaningful. The segmentation logic is mainly contained in the SectionMaster.cs and Section.cs files (within FlashfloodSegmentation folder).

lake-segmented.png and flowers-segmented.png are examples of outputs of the segmentation program (derived from lake.jpg and flowers.jpg respectively), while lake-segmented-partial.png and flowers-segmented-partial.png are examples of a partially completed run of the segmentation program, which included the first divisive step but not the recombination step (as can be seen, the partially processed images contain tens of thousands of sections, whereas the fully processed images contain only a few hundred). 

Written using C# .NET framework and WPF.
