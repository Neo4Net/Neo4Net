using System;

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

namespace Neo4Net.Values
{
    public abstract class AnyValue
    {
        private int _hash;

        // this should be final, but Mockito barfs if it is,
        // so we need to just manually ensure it isn't overridden
        public override bool Equals(object other)
        {
            return this == other || other != null && Eq(other);
        }

        public override sealed int GetHashCode()
        {
            //We will always recompute hashcode for values
            //where `hashCode == 0`, e.g. empty strings and empty lists
            //however that shouldn't be shouldn't be too costly
            if (_hash == 0)
            {
                _hash = ComputeHash();
            }
            return _hash;
        }

        public abstract bool Eq(object other);

        protected internal abstract int ComputeHash();

        public abstract void WriteTo<E>(AnyValueWriter<E> writer) where E : Exception;

        public virtual bool SequenceValue
        {
            get
            {
                return false; // per default Values are no SequenceValues
            }
        }

        public abstract bool? TernaryEquals( AnyValue other );

        public abstract T Map<T>( IValueMapper<T> mapper );

        public abstract string TypeName { get; }
    }
}