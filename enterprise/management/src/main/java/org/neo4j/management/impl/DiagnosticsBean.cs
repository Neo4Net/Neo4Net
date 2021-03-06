﻿using System.Collections.Generic;

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
namespace Org.Neo4j.management.impl
{

	using DiagnosticsManager = Org.Neo4j.@internal.Diagnostics.DiagnosticsManager;
	using DiagnosticsProvider = Org.Neo4j.@internal.Diagnostics.DiagnosticsProvider;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Service = Org.Neo4j.Helpers.Service;
	using ManagementBeanProvider = Org.Neo4j.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Org.Neo4j.Jmx.impl.ManagementData;
	using Neo4jMBean = Org.Neo4j.Jmx.impl.Neo4jMBean;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using FormattedLog = Org.Neo4j.Logging.FormattedLog;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public class DiagnosticsBean extends org.neo4j.jmx.impl.ManagementBeanProvider
	public class DiagnosticsBean : ManagementBeanProvider
	{
		 public DiagnosticsBean() : base(typeof(Diagnostics))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.jmx.impl.Neo4jMBean createMBean(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4jMBean CreateMBean( ManagementData management )
		 {
			  return new DiagnosticsImpl( management );
		 }

		 private class DiagnosticsImpl : Neo4jMBean, Diagnostics
		 {
			  internal readonly DiagnosticsManager Diagnostics;
			  internal Config Config;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: DiagnosticsImpl(org.neo4j.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
			  internal DiagnosticsImpl( ManagementData management ) : base( management )
			  {
					Config = management.ResolveDependency( typeof( Config ) );
					this.Diagnostics = management.ResolveDependency( typeof( DiagnosticsManager ) );
			  }

			  public override void DumpToLog()
			  {
					Diagnostics.dumpAll();
			  }

			  public virtual IList<string> DiagnosticsProviders
			  {
				  get
				  {
						IList<string> result = new List<string>();
						foreach ( DiagnosticsProvider provider in Diagnostics )
						{
							 result.Add( provider.DiagnosticsIdentifier );
						}
						return result;
				  }
			  }

			  public override void DumpToLog( string providerId )
			  {
					Diagnostics.dump( providerId );
			  }

			  public override string DumpAll()
			  {
					StringWriter stringWriter = new StringWriter();
					ZoneId zoneId = Config.get( GraphDatabaseSettings.db_timezone ).ZoneId;
					FormattedLog.Builder logBuilder = FormattedLog.withZoneId( zoneId );
					Diagnostics.dumpAll( logBuilder.ToWriter( stringWriter ) );
					return stringWriter.ToString();
			  }

			  public override string Extract( string providerId )
			  {
					StringWriter stringWriter = new StringWriter();
					ZoneId zoneId = Config.get( GraphDatabaseSettings.db_timezone ).ZoneId;
					FormattedLog.Builder logBuilder = FormattedLog.withZoneId( zoneId );
					Diagnostics.extract( providerId, logBuilder.ToWriter( stringWriter ) );
					return stringWriter.ToString();
			  }
		 }
	}

}