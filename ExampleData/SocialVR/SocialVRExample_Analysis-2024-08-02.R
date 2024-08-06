rm(list = ls())
path <- dirname(rstudioapi::getSourceEditorContext()$path)
path <- paste0(path, "/Data")

# ------------------------------------------------------------------------------
# Load "raw" data, i.e., mere character state data
# These are recorded for every character individually, one line per character
# ------------------------------------------------------------------------------

# Load first file as example
files.raw <- list.files(path, "RawData", full.names = T)
filename.raw <- files.raw[1] 
data.raw <- read.table(filename.raw, dec = ".", sep = ";", skip = 0, header = T, fileEncoding = "latin1")
head(data.raw)

# cut to time when both characters where present
time.start <- min(data.raw$time[data.raw$SelfOrOther == "Other"])
time.end <- max(data.raw$time[data.raw$SelfOrOther == "Other"])
data.raw <- data.raw[data.raw$time >= time.start & data.raw$time <= time.end,]

# Plot head position on birdseye map to just show where characters were sitting
data.raw.self <- data.raw[data.raw$SelfOrOther == "Self",]
data.raw.other <- data.raw[data.raw$SelfOrOther == "Other",]
plot(data.raw.self$headIK.position.x, data.raw.self$headIK.position.z, xlim = c(-1.1,1.1), ylim=c(-1.1,1.1))
points(data.raw.other$headIK.position.x, data.raw.other$headIK.position.z, col = "blue")


# ------------------------------------------------------------------------------
# Load "preprocessed" data, here gaze data from one character transposed relative
# to another character by which 3D data are condensed to 2D representations.
# This process is carried out between the self and all other character and vice versa
# When only one character is in the scene, there are no data because data here
# are always relative to another character
# ------------------------------------------------------------------------------
files.processed <- list.files(path, "PreprocessedData", full.names = T)
filename.processed <- files.processed[1]
data.processed <- read.table(filename.processed, dec = ".", sep = ";", skip = 0, header = T, fileEncoding = "latin1")
head(data.processed)

# Pre-define nice plot in which data from both characters will be shown
library(dplyr)
library(ggplot2)
library(Cairo)
cex <- 18
circle10 <- data.frame(x = 0 + 10 * cos(seq(0, 2 * pi, length.out = 1000)),y = 0 + 10 * sin(seq(0, 2 * pi, length.out = 1000)))
circle20 <- data.frame(x = 0 + 20 * cos(seq(0, 2 * pi, length.out = 1000)),y = 0 + 20 * sin(seq(0, 2 * pi, length.out = 1000)))
circle30 <- data.frame(x = 0 + 30 * cos(seq(0, 2 * pi, length.out = 1000)),y = 0 + 30 * sin(seq(0, 2 * pi, length.out = 1000)))
circle40 <- data.frame(x = 0 + 40 * cos(seq(0, 2 * pi, length.out = 1000)),y = 0 + 40 * sin(seq(0, 2 * pi, length.out = 1000)))
circle50 <- data.frame(x = 0 + 50 * cos(seq(0, 2 * pi, length.out = 1000)),y = 0 + 50 * sin(seq(0, 2 * pi, length.out = 1000)))
circle60 <- data.frame(x = 0 + 60 * cos(seq(0, 2 * pi, length.out = 1000)),y = 0 + 60 * sin(seq(0, 2 * pi, length.out = 1000)))
circle70 <- data.frame(x = 0 + 70 * cos(seq(0, 2 * pi, length.out = 1000)),y = 0 + 70 * sin(seq(0, 2 * pi, length.out = 1000)))
circle80 <- data.frame(x = 0 + 80 * cos(seq(0, 2 * pi, length.out = 1000)),y = 0 + 80 * sin(seq(0, 2 * pi, length.out = 1000)))
circle90 <- data.frame(x = 0 + 90 * cos(seq(0, 2 * pi, length.out = 1000)),y = 0 + 90 * sin(seq(0, 2 * pi, length.out = 1000)))

axisBreaks <- c(-90, -60, -30, -10, 10, 30, 60, 90)
basePlot <- ggplot(thisdat, aes(x = lEye_rEye.x, y = lEye_rEye.y)) + 
  geom_vline(xintercept=0, color = "grey")+
  geom_hline(yintercept=0, color = "grey")+
  geom_path(data = circle10, aes(x,y), color = "grey") +
  geom_path(data = circle20, aes(x,y), color = "grey")+
  geom_path(data = circle30, aes(x,y), color = "grey")+
  geom_path(data = circle40, aes(x,y), color = "grey")+
  geom_path(data = circle50, aes(x,y), color = "grey")+
  geom_path(data = circle60, aes(x,y), color = "grey")+
  theme(legend.position = "none") +
  scale_x_continuous(name = "Horizontral Deviation", breaks = axisBreaks, limits = c(-60,60)) +
  scale_y_continuous(name = "Vertical Deviation", breaks = axisBreaks, limits = c(-60,60))+
  theme(axis.line = element_line(color="black"),
        text = element_text(size=cex, family="Helvetica-Normal"),
        panel.background = element_rect(fill = "white"),
        strip.background= element_rect(fill = "white"),
        legend.text = element_text(size=cex, family="Helvetica-Normal", color = "black"),
        legend.title = element_text(size=cex, family="Helvetica-Normal", color = "black", face="bold"),
        strip.text.x = element_text(size=cex, family="Helvetica-Normal", color = "black",face="bold"),
        axis.text = element_text(size=cex, family="Helvetica-Normal", color = "black"),
        axis.title = element_text(size=cex, family="Helvetica-Normal", color = "black",face="bold"),
        axis.title.y=element_text(margin=margin(0,cex,0,0)))
  

# select one character (with three players, consider ids), and plot its data
WhoseGaze <- "SelfAtOther"
WhoseGaze <- "OtherAtSelf"
thisdat <- data.processed[data.processed$SelfOrOther == WhoseGaze,]

if (WhoseGaze == "SelfAtOther") title <- "Self Looking At Other"
if (WhoseGaze == "OtherAtSelf") title <- "Other Looking At Self"
plot1 <- basePlot + 
  stat_summary_2d(aes(z = 1), fun = function(z) log(sum(z)), bins = 200, alpha = 1) +
  ggtitle(title) + theme(plot.title = element_text(hjust = 0.5)) 
  
plot_filename <- gsub(".csv", paste0("-Gaze", WhoseGaze, ".png"),filename.processed)
plot_filename <- gsub("Data", "Plots",plot_filename)
ggsave(plot_filename, plot = plot1, width = 6.2, height = 6, dpi = 500)


