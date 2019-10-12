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
namespace Neo4Net.Kernel.impl.transaction.log.entry
{

	using Format = Neo4Net.Helpers.Format;

	public abstract class AbstractLogEntry : LogEntry
	{
		public abstract T As();
		 private readonly LogEntryVersion _version;
		 private readonly sbyte _type;

		 internal AbstractLogEntry( LogEntryVersion version, sbyte type )
		 {
			  this._type = type;
			  this._version = version;
		 }

		 public virtual sbyte Type
		 {
			 get
			 {
				  return _type;
			 }
		 }

		 public virtual LogEntryVersion Version
		 {
			 get
			 {
				  return _version;
			 }
		 }

		 public virtual string ToString( TimeZone timeZone )
		 {
			  return ToString();
		 }

		 public override string Timestamp( long timeWritten, TimeZone timeZone )
		 {
			  return Format.date( timeWritten, timeZone ) + "/" + timeWritten;
		 }
	}

}