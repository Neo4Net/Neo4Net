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
namespace Neo4Net.Internal.Kernel.Api
{

	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Neo4Net.Values.Storable;

	/// <summary>
	/// Cursor for scanning the properties of a node or relationship.
	/// </summary>
	public interface PropertyCursor : Cursor
	{
		 int PropertyKey();

		 ValueGroup PropertyType();

		 Value PropertyValue();

		 void writeTo<E>( ValueWriter<E> target );

		 // typed accessor methods

		 bool BooleanValue();

		 string StringValue();

		 long LongValue();

		 double DoubleValue();

		 // Predicates methods that don't require de-serializing the value

		 bool ValueEqualTo( long value );

		 bool ValueEqualTo( double value );

		 bool ValueEqualTo( string value );

		 bool ValueMatches( Pattern regex );

		 bool ValueGreaterThan( long number );

		 bool ValueGreaterThan( double number );

		 bool ValueLessThan( long number );

		 bool ValueLessThan( double number );

		 bool ValueGreaterThanOrEqualTo( long number );

		 bool ValueGreaterThanOrEqualTo( double number );

		 bool ValueLessThanOrEqualTo( long number );

		 bool ValueLessThanOrEqualTo( double number );
	}

}