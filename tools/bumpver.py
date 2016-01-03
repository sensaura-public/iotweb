#!/bin/env python
#----------------------------------------------------------------------------
# 27-Dec-2015 ShaneG
#
# Bump versions for .NET solutions
#----------------------------------------------------------------------------
from os.path import walk, join
from sys import argv
import re

# Regex to find version stamps
VERSION_REGEX = re.compile("([0-9]+\.[0-9]+\.[0-9]+)")

def bumpVersion(line, bumpMinor):
  match = VERSION_REGEX.search(line)
  if not match:
    return line
  # Extract the start, version and end of the line
  first = match.start(1)
  last = match.end(1)
  prefix = line[:first]
  version = line[first:last]
  suffix = line[last:]
  # Split apart the version
  v = list([ int(x) for x in version.split(".") ])
  if bumpMinor:
    v[2] = 0
    v[1] = v[1] + 1
  else:
    v[2] = v[2] + 1
  newver =  ".".join([ str(x) for x in v])
  print "  %s -> %s" % (version, newver)
  return "%s%s%s" % (prefix, newver, suffix)

def AssemblyVersion(filename, bumpMinor):
  print filename
  lines = list()
  for line in open(filename, "r"):
    index = line.find("AssemblyVersion")
    if index >= 0:
      line = bumpVersion(line, bumpMinor)
    index = line.find("AssemblyFileVersion")
    if index >= 0:
      line = bumpVersion(line, bumpMinor)
    lines.append(line)
  # Write the new version
  output = open(filename, "w")
  output.write("".join(lines))
  output.close()

def PackageVersion(filename, bumpMinor):
  print filename
  lines = list()
  for line in open(filename, "r"):
    index = line.find(" Version=")
    if index >= 0:
      line = bumpVersion(line, bumpMinor)
    lines.append(line)
  # Write the new version
  output = open(filename, "w")
  output.write("".join(lines))
  output.close()

def NugetVersion(filename, bumpMinor):
  print filename
  lines = list()
  for line in open(filename, "r"):
    index = line.find("<version>")
    if index >= 0:
      line = bumpVersion(line, bumpMinor)
    lines.append(line)
  # Write the new version
  output = open(filename, "w")
  output.write("".join(lines))
  output.close()

# Map version file names to the handler function
VERFILES = {
  "AssemblyInfo.cs": AssemblyVersion,
  "Package.appxmanifest": PackageVersion,
  "IotWeb.nuspec": NugetVersion,
  }

def checkFile(arg, dirname, names):
  for verfile in VERFILES.keys():
    if verfile in names:
      arg.append((dirname, verfile))

if __name__ == "__main__":
  # Figure out what to bump
  bumpMinor = False
  if (len(argv) == 2) and (argv[1] == "minor"):
    bumpMinor = True
  # Find the files we need to change
  files = list()
  walk(".", checkFile, files)
  for path, name in files:
    fullname = join(path, name)
    VERFILES[name](fullname, bumpMinor)
