////////////using System;

/////////////*
//////////// * Copyright © 2018-2020 "Neo4Net,"
//////////// * Team NeoN [http://neo4net.com]. All Rights Reserved.
//////////// *
//////////// * This file is part of Neo4Net.
//////////// *
//////////// * Neo4Net is free software: you can redistribute it and/or modify
//////////// * it under the terms of the GNU General Public License as published by
//////////// * the Free Software Foundation, either version 3 of the License, or
//////////// * (at your option) any later version.
//////////// *
//////////// * This program is distributed in the hope that it will be useful,
//////////// * but WITHOUT ANY WARRANTY; without even the implied warranty of
//////////// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//////////// * GNU General Public License for more details.
//////////// *
//////////// * You should have received a copy of the GNU General Public License
//////////// * along with this program.  If not, see <http://www.gnu.org/licenses/>.
//////////// */
////////////namespace Neo4Net.GraphDb.config
////////////{
////////////	/// <summary>
////////////	/// Thrown when a configuration setting is, for one reason or another, invalid. </summary>
////////////	/// @deprecated The settings API will be completely rewritten in 4.0 
////////////	[Obsolete("The settings API will be completely rewritten in 4.0")]
////////////	public class InvalidSettingException : Exception
////////////	{

////////////		 public InvalidSettingException( string name, string value, string message ) : base( string.Format( "Bad value '{0}' for setting '{1}': {2}", value, name, message ) )
////////////		 {
////////////		 }

////////////		 public InvalidSettingException( string message ) : base( message )
////////////		 {
////////////		 }

////////////		 public InvalidSettingException( string message, Exception cause ) : base( message, cause )
////////////		 {
////////////		 }
////////////	}

////////////}