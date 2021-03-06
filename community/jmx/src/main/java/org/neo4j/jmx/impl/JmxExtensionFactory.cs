﻿using System;

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
namespace Org.Neo4j.Jmx.impl
{
	using Service = Org.Neo4j.Helpers.Service;
	using Org.Neo4j.Kernel.extension;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using KernelData = Org.Neo4j.Kernel.@internal.KernelData;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(KernelExtensionFactory.class) @Deprecated public final class JmxExtensionFactory extends org.neo4j.kernel.extension.KernelExtensionFactory<JmxExtensionFactory.Dependencies>
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