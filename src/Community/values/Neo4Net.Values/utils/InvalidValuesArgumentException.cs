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
namespace Neo4Net.Values.utils
{
	/// <summary>
	/// {@code InvalidValuesArgumentException} is thrown when trying to pass in an invalid argument to
	/// a {@code PointValue}, {@code TemporalValue} or {@code DurationValue} method. Examples of such
	/// cases include trying to pass an invalid CRS to a {@code PointValue} and trying to pass a
	/// temporal unit out of range when creating a {@code TemporalValue}, e.g. specifying {@code month: 13}.
	/// </summary>
	public class InvalidValuesArgumentException : ValuesException
	{
		 public InvalidValuesArgumentException( string errorMsg ) : base( errorMsg )
		 {
		 }

		 public InvalidValuesArgumentException( string errorMsg, Exception cause ) : base( errorMsg, cause )
		 {
		 }
	}

}