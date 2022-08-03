# -----------------------------------------------------------------------------
# CMake project wrapper Makefile ----------------------------------------------
# -----------------------------------------------------------------------------

SHELL := /bin/bash
RM    := rm -rf
MKDIR := mkdir -p

config = $(shell grep -E '$(1)' config.txt | sed -E 's/.*\s=\s(.*)/\1/')


#	Configuration Files
umlet := $(call config,UMLET_PATH)
inkscape := "$(call config,INKSCAPE_PATH)"


###########################################
# 		SECTION:DOCUMENTATION
###########################################


uxf-files := $(shell find $(doc-path) -name *.uxf)
uxf-file-output := $(foreach file, $(uxf-files),$(subst .uxf,.png,$(file)))
svg-files := $(shell find $(doc-path) -name *.svg)
svg-file-output := $(foreach file, $(svg-files),$(subst .svg,.png,$(file)))



.PHONY: images
images: $(uxf-file-output) $(svg-file-output)
	

%.png: %.uxf
	$(umlet) -action=convert -format=png -filename=$< -output=$@

%.png: %.svg
	$(inkscape) --export-type="png" $<

