/*
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
	using ArrayEncoder = Org.Neo4j.Kernel.Api.Index.ArrayEncoder;
	using Org.Neo4j.Kernel.impl.util;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

	/// <summary>
	/// Validates <seealso cref="Value values"/> that are about to get indexed into a Lucene index.
	/// Values passing this validation are OK to commit and apply to a Lucene index.
	/// </summary>
	public class LuceneIndexValueValidator : Validator<Value>
	{
		 public static readonly LuceneIndexValueValidator Instance = new LuceneIndexValueValidator();

		 // Maximum bytes value length that supported by indexes.
		 // Absolute hard maximum length for a term, in bytes once
		 // encoded as UTF8.  If a term arrives from the analyzer
		 // longer than this length, an IllegalArgumentException
		 // when lucene writer trying to add or update document
		 internal static readonly int MaxTermLength = ( 1 << 15 ) - 2;

		 private readonly IndexTextValueLengthValidator _textValueValidator = new IndexTextValueLengthValidator( MaxTermLength );

		 private LuceneIndexValueValidator()
		 {
		 }

		 public override void Validate( Value value )
		 {
			  _textValueValidator.validate( value );
			  if ( Values.isArrayValue( value ) )
			  {
					_textValueValidator.validate( ArrayEncoder.encode( value ).GetBytes() );
			  }
		 }

		 public virtual void Validate( sbyte[] encodedValue )
		 {
			  _textValueValidator.validate( encodedValue );
		 }
	}

}