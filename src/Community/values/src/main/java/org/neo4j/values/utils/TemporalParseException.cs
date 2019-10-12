using System;

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
namespace Neo4Net.Values.utils
{
	/// <summary>
	/// {@code TemporalParseException} is thrown if parsing of a TemporalValue is unsuccessful.
	/// The constructor parameters {@code parsedData} and {@code errorIndex} can optionally be provided
	/// in order to conform with Java's {@code DateTimeParseException} and {@code SyntaxException}.
	/// </summary>
	public class TemporalParseException : ValuesException
	{
		 private string _parsedData;
		 private int _errorIndex;

		 public TemporalParseException( string errorMsg, Exception cause ) : base( errorMsg, cause )
		 {
		 }

		 public TemporalParseException( string errorMsg, string parsedData, int errorIndex ) : base( errorMsg )
		 {
			  this._parsedData = parsedData;
			  this._errorIndex = errorIndex;
		 }

		 public TemporalParseException( string errorMsg, string parsedData, int errorIndex, Exception cause ) : base( errorMsg, cause )
		 {
			  this._parsedData = parsedData;
			  this._errorIndex = errorIndex;
		 }

		 public virtual string ParsedData
		 {
			 get
			 {
				  return _parsedData;
			 }
		 }

		 public virtual int ErrorIndex
		 {
			 get
			 {
				  return _errorIndex;
			 }
		 }
	}

}