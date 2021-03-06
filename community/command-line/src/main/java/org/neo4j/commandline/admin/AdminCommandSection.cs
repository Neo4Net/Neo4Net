﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Commandline.admin
{

	public abstract class AdminCommandSection
	{
		 private static readonly AdminCommandSection _general = new GeneralSection();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public abstract String printable();
		 public abstract string Printable();

		 public static AdminCommandSection General()
		 {
			  return _general;
		 }

		 public override int GetHashCode()
		 {
			  return this.Printable().GetHashCode();
		 }

		 public override bool Equals( object other )
		 {
			  return other is AdminCommandSection && this.Printable().Equals(((AdminCommandSection) other).Printable());
		 }

		 public void PrintAllCommandsUnderSection( System.Action<string> output, IList<AdminCommand_Provider> providers )
		 {
			  output( "" );
			  output( Printable() );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  providers.sort( System.Collections.IComparer.comparing( AdminCommand_Provider::name ) );
			  providers.ForEach( provider => provider.printSummary( s => output( "    " + s ) ) );
		 }

		 internal class GeneralSection : AdminCommandSection
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String printable()
			  public override string Printable()
			  {
					return "General";
			  }
		 }
	}

}