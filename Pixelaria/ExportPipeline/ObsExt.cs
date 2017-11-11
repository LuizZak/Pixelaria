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
using System.IO;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Pixelaria.ExportPipeline
{
    public static class ObsExt
    {
        /// <summary>
        /// Helper for debugging observable sequences.
        /// 
        /// Reports with <see cref="Console.WriteLine(string)"/> and <see cref="System.Diagnostics.Debug.WriteLine(string)"/>
        /// every time an OnNext, OnCompleted and OnError event are produced by the original observable sequence.
        /// 
        /// The resulting sequence produces the same events/elements as the original sequence.
        /// </summary>
        public static IObservable<T> Debug<T>(this IObservable<T> obs, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string fileName = Path.GetFileName(sourceFilePath);
            
            string sourceLog = $"{fileName} [{memberName}:{sourceLineNumber}] <{typeof(T).Name}>";

            return obs.Do(next =>
            {
                Console.WriteLine($@"{sourceLog} OnNext: {next}");
                System.Diagnostics.Debug.WriteLine($"{sourceLog} OnNext: {next}");
            }, error =>
            {
                Console.WriteLine($@"{sourceLog} OnError: {error}");
                System.Diagnostics.Debug.WriteLine($"{sourceLog} OnError: {error}");
            }, () =>
            {
                Console.WriteLine($@"{sourceLog} OnCompleted");
                System.Diagnostics.Debug.WriteLine($"{sourceLog} OnCompleted");
            });
        }

        /// <summary>
        /// A version of "WithLatestFrom" with a more tradicional and predictable behavior.
        /// </summary>
        public static IObservable<TResult> PxlWithLatestFrom<TLeft, TRight, TResult>(this IObservable<TLeft> source, IObservable<TRight> other, [NotNull] Func<TLeft, TRight, TResult> resultSelector)
        {
            return
                source.Publish(os =>
                {
                    return
                        other
                            .Select(a => os
                                .Select(b => resultSelector(b, a))).Switch();
                });
        }
    }
}