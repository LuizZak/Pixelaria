/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System;

namespace Blend2DCS.Internal
{
    internal static class Exceptions
    {
        public static void ThrowOnError(uint result)
        {
            var resultCode = (BLResultCode) result;

            if (resultCode != BLResultCode.Success)
            {
                throw new Blend2DException(resultCode);
            }
        }
    }

    public class Blend2DException : Exception
    {
        public BLResultCode ResultCode { get; }

        public Blend2DException(BLResultCode resultCode) : base($"Blend2D exception: {resultCode.ToString()}")
        {
            ResultCode = resultCode;
        }
    }

    /// <summary>
    /// Blend2D result code.
    /// </summary>
    public enum BLResultCode : uint
    {
        /// <summary>
        /// Successful result code.
        /// </summary>
        Success = 0,

        ErrorStartIndex = 0x00010000u,

        ErrorOutOfMemory = 0x00010000u,  //!< Out of memory                 [ENOMEM].
        ErrorInvalidValue,                //!< Invalid value/argument        [EINVAL].
        ErrorInvalidState,                //!< Invalid state                 [EFAULT].
        ErrorInvalidHandle,               //!< Invalid handle or file.       [EBADF].
        ErrorValueTooLarge,              //!< Value too large               [EOVERFLOW].
        ErrorNotInitialized,              //!< Not initialized (some instance is built-in none when it shouldn't be).
        ErrorNotImplemented,              //!< Not implemented               [ENOSYS].
        ErrorNotPermitted,                //!< Operation not permitted       [EPERM].

        ErrorIo,                           //!< IO error                      [EIO].
        ErrorBusy,                         //!< Device or resource busy       [EBUSY].
        ErrorInterrupted,                  //!< Operation interrupted         [EINTR].
        ErrorTryAgain,                    //!< Try again                     [EAGAIN].
        ErrorTimedOut,                    //!< Timed out                     [ETIMEDOUT].
        ErrorBrokenPipe,                  //!< Broken pipe                   [EPIPE].
        ErrorInvalidSeek,                 //!< File is not seekable          [ESPIPE].
        ErrorSymlinkLoop,                 //!< Too many levels of symlinks   [ELOOP].
        ErrorFileTooLarge,               //!< File is too large             [EFBIG].
        ErrorAlreadyExists,               //!< File/directory already exists [EEXIST].
        ErrorAccessDenied,                //!< Access denied                 [EACCES].
        ErrorMediaChanged,                //!< Media changed                 [Windows::ErrorMediaChanged].
        ErrorReadOnlyFs,                 //!< The file/FS is read-only      [EROFS].
        ErrorNoDevice,                    //!< Device doesn't exist          [ENXIO].
        ErrorNoEntry,                     //!< Not found, no entry (fs)      [ENOENT].
        ErrorNoMedia,                     //!< No media in drive/device      [ENOMEDIUM].
        ErrorNoMoreData,                 //!< No more data / end of file    [ENODATA].
        ErrorNoMoreFiles,                //!< No more files                 [ENMFILE].
        ErrorNoSpaceLeft,                //!< No space left on device       [ENOSPC].
        ErrorNotEmpty,                    //!< Directory is not empty        [ENOTEMPTY].
        ErrorNotFile,                     //!< Not a file                    [EISDIR].
        ErrorNotDirectory,                //!< Not a directory               [ENOTDIR].
        ErrorNotSameDevice,              //!< Not same device               [EXDEV].
        ErrorNotBlockDevice,             //!< Not a block device            [ENOTBLK].

        ErrorInvalidFileName,            //!< File/path name is invalid     [n/a].
        ErrorFileNameTooLong,           //!< File/path name is too long    [ENAMETOOLONG].

        ErrorTooManyOpenFiles,          //!< Too many open files           [EMFILE].
        ErrorTooManyOpenFilesByOs,    //!< Too many open files by OS     [ENFILE].
        ErrorTooManyLinks,               //!< Too many symbolic links on FS [EMLINK].
        ErrorTooManyThreads,             //!< Too many threads              [EAGAIN].

        ErrorFileEmpty,                   //!< File is empty (not specific to any OS error).
        ErrorOpenFailed,                  //!< File open failed              [Windows::ErrorOpenFailed].
        ErrorNotRootDevice,              //!< Not a root device/directory   [Windows::ErrorDirNotRoot].

        ErrorUnknownSystemError,         //!< Unknown system error that failed to translate to Blend2D result code.

        ErrorInvalidAlignment,            //!< Invalid data alignment.
        ErrorInvalidSignature,            //!< Invalid data signature or header.
        ErrorInvalidData,                 //!< Invalid or corrupted data.
        ErrorInvalidString,               //!< Invalid string (invalid data of either UTF8, UTF16, or UTF32).
        ErrorDataTruncated,               //!< Truncated data (more data required than memory/stream provides).
        ErrorDataTooLarge,               //!< Input data too large to be processed.
        ErrorDecompressionFailed,         //!< Decompression failed due to invalid data (RLE, Huffman, etc).

        ErrorInvalidGeometry,             //!< Invalid geometry (invalid path data or shape).
        ErrorNoMatchingVertex,           //!< Returned when there is no matching vertex in path data.

        ErrorNoMatchingCookie,           //!< No matching cookie (BLContext).
        ErrorNoStatesToRestore,         //!< No states to restore (BLContext).

        ErrorImageTooLarge,              //!< The size of the image is too large.
        ErrorImageNoMatchingCodec,      //!< Image codec for a required format doesn't exist.
        ErrorImageUnknownFileFormat,    //!< Unknown or invalid file format that cannot be read.
        ErrorImageDecoderNotProvided,   //!< Image codec doesn't support reading the file format.
        ErrorImageEncoderNotProvided,   //!< Image codec doesn't support writing the file format.

        ErrorPngMultipleIhdr,            //!< Multiple IHDR chunks are not allowed (PNG).
        ErrorPngInvalidIdat,             //!< Invalid IDAT chunk (PNG).
        ErrorPngInvalidIend,             //!< Invalid IEND chunk (PNG).
        ErrorPngInvalidPlte,             //!< Invalid PLTE chunk (PNG).
        ErrorPngInvalidTrns,             //!< Invalid tRNS chunk (PNG).
        ErrorPngInvalidFilter,           //!< Invalid filter type (PNG).

        ErrorJpegUnsupportedFeature,     //!< Unsupported feature (JPEG).
        ErrorJpegInvalidSos,             //!< Invalid SOS marker or header (JPEG).
        ErrorJpegInvalidSof,             //!< Invalid SOF marker (JPEG).
        ErrorJpegMultipleSof,            //!< Multiple SOF markers (JPEG).
        ErrorJpegUnsupportedSof,         //!< Unsupported SOF marker (JPEG).

        ErrorFontNoCharacterMapping,    //!< Font has no character to glyph mapping data.
        ErrorFontMissingImportantTable, //!< Font has missing an important table.
        ErrorFontFeatureNotAvailable,   //!< Font feature is not available.
        ErrorFontCffInvalidData,        //!< Font has an invalid CFF data.
        ErrorFontProgramTerminated,      //!< Font program terminated because the execution reached the limit.

        ErrorInvalidGlyph                 //!< Invalid glyph identifier.
    }
}
