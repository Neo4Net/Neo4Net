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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using PhysicalLogCommandReaderV2_2_10 = Neo4Net.Kernel.impl.transaction.command.PhysicalLogCommandReaderV2_2_10;
	using PhysicalLogCommandReaderV2_2_4 = Neo4Net.Kernel.impl.transaction.command.PhysicalLogCommandReaderV2_2_4;
	using PhysicalLogCommandReaderV3_0 = Neo4Net.Kernel.impl.transaction.command.PhysicalLogCommandReaderV3_0;
	using PhysicalLogCommandReaderV3_0_2 = Neo4Net.Kernel.impl.transaction.command.PhysicalLogCommandReaderV3_0_2;
	using LogEntryVersion = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using CommandReader = Neo4Net.Storageengine.Api.CommandReader;
	using CommandReaderFactory = Neo4Net.Storageengine.Api.CommandReaderFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.abs;

	public class RecordStorageCommandReaderFactory : CommandReaderFactory
	{
		 // All supported readers. Key/index is LogEntryVersion byte code.
		 private readonly CommandReader[] _readers;

		 public RecordStorageCommandReaderFactory()
		 {
			  _readers = new CommandReader[11]; // pessimistic size
			  _readers[-LogEntryVersion.V2_3.byteCode()] = new PhysicalLogCommandReaderV2_2_4();
			  _readers[-LogEntryVersion.V3_0.byteCode()] = new PhysicalLogCommandReaderV3_0();
			  _readers[-LogEntryVersion.V2_3_5.byteCode()] = new PhysicalLogCommandReaderV2_2_10();
			  _readers[-LogEntryVersion.V3_0_2.byteCode()] = new PhysicalLogCommandReaderV3_0_2();
			  // The 3_0_10 version bump is only to prevent mixed-version clusters; format is otherwise backwards compatible.
			  _readers[-LogEntryVersion.V3_0_10.byteCode()] = new PhysicalLogCommandReaderV3_0_2();

			  // A little extra safety check so that we got 'em all
			  LogEntryVersion[] versions = LogEntryVersion.values();
			  foreach ( LogEntryVersion version in versions )
			  {
					CommandReader versionReader = _readers[abs( version.byteCode() )];
					if ( versionReader == null )
					{
						 throw new System.InvalidOperationException( "Version " + version + " not handled" );
					}
			  }
		 }

		 public override CommandReader ByVersion( sbyte version )
		 {
			  sbyte key = ( sbyte ) abs( version );
			  if ( key >= _readers.Length )
			  {
					throw new System.ArgumentException( "Unsupported version:" + version );
			  }
			  return _readers[key];
		 }
	}

}