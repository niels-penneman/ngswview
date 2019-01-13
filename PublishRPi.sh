#!/bin/bash
#
# Script to build a .deb package for deployment to a Raspberry Pi.
#
set -e
CONFIGURATION='Release'
FRAMEWORK='netcoreapp2.0'
RUNTIME='linux-arm'
ARGS="-c ${CONFIGURATION} -f ${FRAMEWORK} -r ${RUNTIME}"
find "./bin/${CONFIGURATION}/${FRAMEWORK}/${RUNTIME}/" -name "ngswview.*.${RUNTIME}.deb" -delete
dotnet restore -r ${RUNTIME}
dotnet clean ${ARGS}
dotnet build ${ARGS}
dotnet deb ${ARGS}
DEBFILE=`find "./bin/${CONFIGURATION}/${FRAMEWORK}/${RUNTIME}/" -name "ngswview.*.${RUNTIME}.deb"`
rm -rf .rpi.deb
mkdir .rpi.deb
cd .rpi.deb
ar p "../${DEBFILE}" control.tar.gz | tar -xz
sed -i'' 's/\(Architecture: \)amd64/\1armhf/' control
tar czf control.tar.gz *[!z]
ar r "../${DEBFILE}" control.tar.gz
cd -
rm -rf .rpi.deb
echo "Package file: ${DEBFILE}"
