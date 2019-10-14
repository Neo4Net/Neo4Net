/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.metrics.source.jvm
{
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	[Documented("=== Java Virtual Machine Metrics\n\n" + "These metrics are environment dependent and they may vary on different hardware and with JVM configurations.\n" + "Typically these metrics will show information about garbage collections " + "(for example the number of events and time spent collecting), memory pools and buffers, and " + "finally the number of active threads running.")]
	public abstract class JvmMetrics : LifecycleAdapter
	{
		 public const string NAME_PREFIX = "vm";

		 public static string PrettifyName( string name )
		 {
			  return name.ToLower().Replace(' ', '_');
		 }
	}

}