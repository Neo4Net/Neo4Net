﻿/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using Org.Neo4j.Kernel.impl.util;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

	public abstract class AbstractIndexKeyLengthValidator : Validator<Value>
	{
		public abstract void Validate( T value );
		 protected internal readonly int MaxByteLength;
		 private readonly int _checkThreshold;

		 protected internal AbstractIndexKeyLengthValidator( int maxByteLength )
		 {
			  this.MaxByteLength = maxByteLength;

			  // This check threshold is for not having to check every value that comes in, only those that may have a chance to exceed the max length.
			  // The value 5 comes from a safer 4, which is the number of bytes that a max size UTF-8 code point needs.
			  this._checkThreshold = maxByteLength / 5;
		 }

		 public override void Validate( Value value )
		 {
			  if ( value == null || value == Values.NO_VALUE )
			  {
					throw new System.ArgumentException( "Null value" );
			  }
			  if ( Values.isTextValue( value ) && ( ( TextValue )value ).length() >= _checkThreshold )
			  {
					int length = IndexKeyLength( value );
					ValidateLength( length );
			  }
		 }

		 internal virtual void ValidateLength( int byteLength )
		 {
			  if ( byteLength > MaxByteLength )
			  {
					throw new System.ArgumentException( "Property value size is too large for index. Please see index documentation for limitations." );
			  }
		 }

		 protected internal abstract int IndexKeyLength( Value value );
	}

}