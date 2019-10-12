using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.backup.impl
{

	public class BackupHelpOutput
	{
		 public static readonly IList<string> BackupOutputLines = BackupOutput();

		 private static IList<string> BackupOutput()
		 {
			  IList<string> lines = new List<string>();

			  lines.Add( "usage: neo4j-admin backup --backup-dir=<backup-path> --name=<graph.db-backup>" );
			  lines.Add( "                          [--from=<address>] [--protocol=<any|catchup|common>]" );
			  lines.Add( "                          [--fallback-to-full[=<true|false>]]" );
			  lines.Add( "                          [--timeout=<timeout>] [--pagecache=<8m>]" );
			  lines.Add( "                          [--check-consistency[=<true|false>]]" );
			  lines.Add( "                          [--cc-report-dir=<directory>]" );
			  lines.Add( "                          [--additional-config=<config-file-path>]" );
			  lines.Add( "                          [--cc-graph[=<true|false>]]" );
			  lines.Add( "                          [--cc-indexes[=<true|false>]]" );
			  lines.Add( "                          [--cc-label-scan-store[=<true|false>]]" );
			  lines.Add( "                          [--cc-property-owners[=<true|false>]]" );
			  lines.Add( "" );
			  lines.Add( "environment variables:" );
			  lines.Add( "    NEO4J_CONF    Path to directory which contains neo4j.conf." );
			  lines.Add( "    NEO4J_DEBUG   Set to anything to enable debug output." );
			  lines.Add( "    NEO4J_HOME    Neo4j home directory." );
			  lines.Add( "    HEAP_SIZE     Set JVM maximum heap size during command execution." );
			  lines.Add( "                  Takes a number and a unit, for example 512m." );
			  lines.Add( "" );
			  lines.Add( "Perform an online backup from a running Neo4j enterprise server. Neo4j's backup" );
			  lines.Add( "service must have been configured on the server beforehand." );
			  lines.Add( "" );
			  lines.Add( "All consistency checks except 'cc-graph' can be quite expensive so it may be" );
			  lines.Add( "useful to turn them off for very large databases. Increasing the heap size can" );
			  lines.Add( "also be a good idea. See 'neo4j-admin help' for details." );
			  lines.Add( "" );
			  lines.Add( "For more information see:" );
			  lines.Add( "https://neo4j.com/docs/operations-manual/current/backup/" );
			  lines.Add( "" );
			  lines.Add( "options:" );
			  lines.Add( "  --backup-dir=<backup-path>               Directory to place backup in." );
			  lines.Add( "  --name=<graph.db-backup>                 Name of backup. If a backup with this" );
			  lines.Add( "                                           name already exists an incremental" );
			  lines.Add( "                                           backup will be attempted." );
			  lines.Add( "  --from=<address>                         Host and port of Neo4j." );
			  lines.Add( "                                           [default:localhost:6362]" );
			  lines.Add( "  --protocol=<any|catchup|common>          Preferred backup protocol" );
			  lines.Add( "                                           [default:any]" );
			  lines.Add( "  --fallback-to-full=<true|false>          If an incremental backup fails backup" );
			  lines.Add( "                                           will move the old backup to" );
			  lines.Add( "                                           <name>.err.<N> and fallback to a full" );
			  lines.Add( "                                           backup instead. [default:true]" );
			  lines.Add( "  --timeout=<timeout>                      Timeout in the form <time>[ms|s|m|h]," );
			  lines.Add( "                                           where the default unit is seconds." );
			  lines.Add( "                                           [default:20m]" );
			  lines.Add( "  --pagecache=<8m>                         The size of the page cache to use for" );
			  lines.Add( "                                           the backup process. [default:8m]" );
			  lines.Add( "  --check-consistency=<true|false>         If a consistency check should be" );
			  lines.Add( "                                           made. [default:true]" );
			  lines.Add( "  --cc-report-dir=<directory>              Directory where consistency report" );
			  lines.Add( "                                           will be written. [default:.]" );
			  lines.Add( "  --additional-config=<config-file-path>   Configuration file to supply" );
			  lines.Add( "                                           additional configuration in. This" );
			  lines.Add( "                                           argument is DEPRECATED. [default:]" );
			  lines.Add( "  --cc-graph=<true|false>                  Perform consistency checks between" );
			  lines.Add( "                                           nodes, relationships, properties," );
			  lines.Add( "                                           types and tokens. [default:true]" );
			  lines.Add( "  --cc-indexes=<true|false>                Perform consistency checks on" );
			  lines.Add( "                                           indexes. [default:true]" );
			  lines.Add( "  --cc-label-scan-store=<true|false>       Perform consistency checks on the" );
			  lines.Add( "                                           label scan store. [default:true]" );
			  lines.Add( "  --cc-property-owners=<true|false>        Perform additional consistency checks" );
			  lines.Add( "                                           on property ownership. This check is" );
			  lines.Add( "                                           *very* expensive in time and memory." );
			  lines.Add( "                                           [default:false]" );
			  string platformNewLine = Environment.NewLine;
			  lines = lines.Select( line => line += platformNewLine ).ToList();
			  return lines;
		 }
	}


}