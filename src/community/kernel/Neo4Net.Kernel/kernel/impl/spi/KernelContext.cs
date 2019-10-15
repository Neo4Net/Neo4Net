﻿using System;

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
namespace Neo4Net.Kernel.impl.spi
{

	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using DependencySatisfier = Neo4Net.Kernel.impl.util.DependencySatisfier;

	public interface KernelContext
	{
		 /// <returns> store directory for <seealso cref="ExtensionType.GLOBAL"/> extensions and
		 /// particular database directory if extension is per <seealso cref="ExtensionType.DATABASE"/>. </returns>
		 /// @deprecated Please use <seealso cref="directory()"/> instead. 
		 [Obsolete("Please use <seealso cref=\"directory()\"/> instead.")]
		 File StoreDir();

		 DatabaseInfo DatabaseInfo();

		 DependencySatisfier DependencySatisfier();

		 /// <returns> store directory for <seealso cref="ExtensionType.GLOBAL"/> extensions and
		 /// particular database directory if extension is per <seealso cref="ExtensionType.DATABASE"/>. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.io.File directory()
	//	 {
	//		  return storeDir();
	//	 }
	}

}