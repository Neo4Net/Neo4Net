using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.management
{

	using Description = Neo4Net.Jmx.Description;
	using ManagementInterface = Neo4Net.Jmx.ManagementInterface;

	[ManagementInterface(name : Diagnostics_Fields.NAME), Description("Diagnostics provided by Neo4Net")]
	public interface Diagnostics
	{

		 [Description("Dump diagnostics information to the log.")]
		 void DumpToLog();

		 [Description("Dump diagnostics information for the diagnostics provider with the specified id.")]
		 void DumpToLog( string providerId );

		 [Description("Dump diagnostics information to JMX")]
		 string DumpAll();

		 [Description("Extract diagnostics information for the diagnostics provider with the specified id.")]
		 string Extract( string providerId );

		 [Description("A list of the ids for the registered diagnostics providers.")]
		 IList<string> DiagnosticsProviders { get; }
	}

	public static class Diagnostics_Fields
	{
		 public const string NAME = "Diagnostics";
	}

}