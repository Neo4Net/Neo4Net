using System;

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
namespace Org.Neo4j.Kernel.impl.spi
{

	using ExtensionType = Org.Neo4j.Kernel.extension.ExtensionType;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using DependencySatisfier = Org.Neo4j.Kernel.impl.util.DependencySatisfier;

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