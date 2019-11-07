/*
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
namespace Neo4Net.Ext.Udc.impl
{

	using Service = Neo4Net.Helpers.Service;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.extension;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using UsageData = Neo4Net.Udc.UsageData;

	/// <summary>
	/// Kernel extension for UDC, the Usage Data Collector. The UDC runs as a background
	/// daemon, waking up once a day to collect basic usage information about a long
	/// running graph database.
	/// <para>
	/// The first update is delayed to avoid needless activity during integration
	/// testing and short-run applications. Subsequent updates are made at regular
	/// intervals. Both times are specified in milliseconds.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(KernelExtensionFactory.class) public class UdcKernelExtensionFactory extends Neo4Net.kernel.extension.KernelExtensionFactory<UdcKernelExtensionFactory.Dependencies>
	public class UdcKernelExtensionFactory : KernelExtensionFactory<UdcKernelExtensionFactory.Dependencies>
	{
		 internal const string KEY = "kernel udc";

		 public interface Dependencies
		 {
			  Config Config();
			  DataSourceManager DataSourceManager();
			  UsageData UsageData();
		 }

		 public UdcKernelExtensionFactory() : base(KEY)
		 {
		 }

		 public override Lifecycle NewInstance( KernelContext kernelContext, UdcKernelExtensionFactory.Dependencies dependencies )
		 {
			  Config config = dependencies.Config();
			  return new UdcKernelExtension( config, dependencies.DataSourceManager(), dependencies.UsageData(), new Timer("Neo4Net UDC Timer", true) );
		 }
	}

}