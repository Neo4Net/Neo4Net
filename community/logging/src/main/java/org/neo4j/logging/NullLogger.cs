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
namespace Org.Neo4j.Logging
{

	/// <summary>
	/// A <seealso cref="Logger"/> implementation that discards all messages
	/// </summary>
	public sealed class NullLogger : Logger
	{
		 private static readonly NullLogger _instance = new NullLogger();

		 private NullLogger()
		 {
		 }

		 /// <returns> A singleton <seealso cref="NullLogger"/> instance </returns>
		 public static NullLogger Instance
		 {
			 get
			 {
				  return _instance;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message)
		 public override void Log( string message )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message, @Nonnull Throwable throwable)
		 public override void Log( string message, Exception throwable )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String format, @Nonnull Object... arguments)
		 public override void Log( string format, params object[] arguments )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<Logger> consumer)
		 public override void Bulk( Consumer<Logger> consumer )
		 {
		 }
	}

}