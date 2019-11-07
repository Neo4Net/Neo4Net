﻿using System;

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
	/// {@code UnsupportedTemporalUnitException} is thrown if trying to get or assign a temporal unit
	/// which is not supported for the current temporal type. Examples of such cases include trying to
	/// assign a month to a {@code TimeValue}, trying to truncate a {@code DateValue} to minutes and
	/// trying to get the timezone of a {@code LocalDateTimeValue}.
	/// </summary>
	public class UnsupportedTemporalUnitException : ValuesException
	{
		 public UnsupportedTemporalUnitException( string errorMsg ) : base( errorMsg )
		 {
		 }

		 public UnsupportedTemporalUnitException( string errorMsg, Exception cause ) : base( errorMsg, cause )
		 {
		 }
	}

}