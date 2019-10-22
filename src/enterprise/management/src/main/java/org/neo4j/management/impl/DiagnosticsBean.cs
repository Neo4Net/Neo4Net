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
namespace Neo4Net.management.impl
{

	using DiagnosticsManager = Neo4Net.Internal.Diagnostics.DiagnosticsManager;
	using DiagnosticsProvider = Neo4Net.Internal.Diagnostics.DiagnosticsProvider;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Service = Neo4Net.Helpers.Service;
	using ManagementBeanProvider = Neo4Net.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using Neo4NetMBean = Neo4Net.Jmx.impl.Neo4NetMBean;
	using Config = Neo4Net.Kernel.configuration.Config;
	using FormattedLog = Neo4Net.Logging.FormattedLog;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public class DiagnosticsBean extends org.Neo4Net.jmx.impl.ManagementBeanProvider
	public class DiagnosticsBean : ManagementBeanProvider
	{
		 public DiagnosticsBean() : base(typeof(Diagnostics))
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.jmx.impl.Neo4NetMBean createMBean(org.Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
		 protected internal override Neo4NetMBean CreateMBean( ManagementData management )
		 {
			  return new DiagnosticsImpl( management );
		 }

		 private class DiagnosticsImpl : Neo4NetMBean, Diagnostics
		 {
			  internal readonly DiagnosticsManager Diagnostics;
			  internal Config Config;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: DiagnosticsImpl(org.Neo4Net.jmx.impl.ManagementData management) throws javax.management.NotCompliantMBeanException
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