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
namespace Neo4Net.Kernel.impl.factory
{
	public class DatabaseInfo
	{
		 public static readonly DatabaseInfo Unknown = new DatabaseInfo( Edition.Unknown, OperationalMode.Unknown );
		 public static readonly DatabaseInfo Tool = new DatabaseInfo( Edition.Unknown, OperationalMode.Single );
		 public static readonly DatabaseInfo Community = new DatabaseInfo( Edition.Community, OperationalMode.Single );
		 public static readonly DatabaseInfo Enterprise = new DatabaseInfo( Edition.Enterprise, OperationalMode.Single );
		 public static readonly DatabaseInfo Ha = new DatabaseInfo( Edition.Enterprise, OperationalMode.Ha );
		 public static readonly DatabaseInfo Core = new DatabaseInfo( Edition.Enterprise, OperationalMode.Core );
		 public static readonly DatabaseInfo ReadReplica = new DatabaseInfo( Edition.Enterprise, OperationalMode.ReadReplica );

		 public readonly Edition Edition;
		 public readonly OperationalMode OperationalMode;

		 private DatabaseInfo( Edition edition, OperationalMode operationalMode )
		 {
			  this.Edition = edition;
			  this.OperationalMode = operationalMode;
		 }

		 public override string ToString()
		 {
			  return Edition + " " + OperationalMode;
		 }

	}

}