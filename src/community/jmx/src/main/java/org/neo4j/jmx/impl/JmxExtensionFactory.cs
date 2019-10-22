using System;

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
namespace Neo4Net.Jmx.impl
{
	using Service = Neo4Net.Helpers.Service;
	using Neo4Net.Kernel.extension;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using KernelData = Neo4Net.Kernel.Internal.KernelData;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LogService = Neo4Net.Logging.Internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(KernelExtensionFactory.class) @Deprecated public final class JmxExtensionFactory extends org.Neo4Net.kernel.extension.KernelExtensionFactory<JmxExtensionFactory.Dependencies>
	[Obsolete]
	public sealed class JmxExtensionFactory : KernelExtensionFactory<JmxExtensionFactory.Dependencies>
	{
		 public interface Dependencies
		 {
			  KernelData KernelData { get; }

			  LogService LogService { get; }

			  DataSourceManager DataSourceManager { get; }
		 }

		 public const string KEY = "kernel jmx";

		 public JmxExtensionFactory() : base(KEY)
		 {
		 }

		 public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
		 {
			  return new JmxKernelExtension( dependencies.KernelData, dependencies.DataSourceManager, dependencies.LogService.InternalLogProvider );
		 }
	}

}