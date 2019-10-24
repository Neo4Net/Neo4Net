﻿using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Collections
{
    public abstract class PrefetchingRawIterator<T, EXCEPTION> : RawIterator<T, EXCEPTION> where EXCEPTION : Exception
    {
        public abstract RawIterator<T, EX> Wrap(IEnumerator<T> iterator);
        public abstract RawIterator<T, EX> From(Neo4Net.Functions.ThrowingSupplier<T, EX> supplier);
        public abstract RawIterator<T, EX> Of(params T[] values);
        public abstract RawIterator<T, EXCEPTION> Empty();
        internal bool HasFetchedNext;
        internal T NextObject;

        /// <returns> {@code true} if there is a next item to be returned from the next
        /// call to <seealso cref="next()"/>. </returns>
        public override bool HasNext()
        {
            return Peek() != default(T);
        }

        /// <returns> the next element that will be returned from <seealso cref="next()"/> without
        /// actually advancing the iterator </returns>
        public virtual T Peek()
        {
            if (HasFetchedNext)
            {
                return NextObject;
            }

            NextObject = FetchNextOrNull();
            HasFetchedNext = true;
            return NextObject;
        }

        /// <summary>
        /// Uses <seealso cref="hasNext()"/> to try to fetch the next item and returns it
        /// if found, otherwise it throws a <seealso cref="java.util.NoSuchElementException"/>.
        /// </summary>
        /// <returns> the next item in the iteration, or throws
        /// <seealso cref="java.util.NoSuchElementException"/> if there's no more items to return. </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: public T next() throws EXCEPTION
        public override T Next()
        {
            if (!HasNext())
            {
                throw new NoSuchElementException();
            }
            T result = NextObject;
            NextObject = default(T);
            HasFetchedNext = false;
            return result;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: protected abstract T fetchNextOrNull() throws EXCEPTION;
        protected internal abstract T FetchNextOrNull();

        public override void Remove()
        {
            throw new System.NotSupportedException();
        }
    }
}