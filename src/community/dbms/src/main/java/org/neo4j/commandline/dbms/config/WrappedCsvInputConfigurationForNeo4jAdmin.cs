﻿/*
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
namespace Neo4Net.CommandLine.dbms.config
{
	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration;
	/// <summary>
	/// Provides a wrapper around <seealso cref="Configuration"/> with overridden defaults for neo4j-admin import
	/// Always trim strings
	/// Import emptyQuotedStrings as empty Strings
	/// Buffer size is set to 4MB
	/// </summary>

	public class WrappedCsvInputConfigurationForNeo4jAdmin : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
	{
		 public WrappedCsvInputConfigurationForNeo4jAdmin( Configuration defaults ) : base( defaults )
		 {
		 }

		 public override int BufferSize()
		 {
			  return DEFAULT_BUFFER_SIZE_4MB;
		 }

		 public override bool TrimStrings()
		 {
			  return true;
		 }

		 public override bool EmptyQuotedStringsAsNull()
		 {
			  return false;
		 }

		 public override bool LegacyStyleQuoting()
		 {
			  return false;
		 }
	}

}