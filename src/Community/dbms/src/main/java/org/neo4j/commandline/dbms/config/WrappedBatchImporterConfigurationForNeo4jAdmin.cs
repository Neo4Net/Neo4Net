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
namespace Neo4Net.CommandLine.dbms.config
{
	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.Configuration;

	/// <summary>
	/// Provides a wrapper around <seealso cref="Configuration"/> with overridden defaults for neo4j-admin import
	/// Use all available processors
	/// </summary>
	public class WrappedBatchImporterConfigurationForNeo4jAdmin : Neo4Net.@unsafe.Impl.Batchimport.Configuration_Overridden
	{
		 public WrappedBatchImporterConfigurationForNeo4jAdmin( Configuration defaults ) : base( defaults )
		 {
		 }

		 public override int MaxNumberOfProcessors()
		 {
			  return Configuration.allAvailableProcessors();
		 }
	}

}