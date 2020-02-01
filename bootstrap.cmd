cd blend2d
mkdir blend2d\build
cd blend2d\build

cmake .. -DCMAKE_BUILD_TYPE=Release -DBLEND2D_TEST=TRUE
cmake --build .
