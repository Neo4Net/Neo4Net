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
namespace Neo4Net.Dbms.archive
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class ArchiveProgressPrinterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void progressOutput()
		 internal virtual void ProgressOutput()
		 {
			  MemoryStream bout = new MemoryStream();
			  PrintStream printStream = new PrintStream( bout );
			  ArchiveProgressPrinter progressPrinter = new ArchiveProgressPrinter( printStream );
			  progressPrinter.MaxBytes = 1000;
			  progressPrinter.MaxFiles = 10;

			  progressPrinter.BeginFile();
			  progressPrinter.AddBytes( 5 );
			  progressPrinter.EndFile();
			  progressPrinter.BeginFile();
			  progressPrinter.AddBytes( 50 );
			  progressPrinter.AddBytes( 50 );
			  progressPrinter.PrintOnNextUpdate();
			  progressPrinter.AddBytes( 100 );
			  progressPrinter.EndFile();
			  progressPrinter.BeginFile();
			  progressPrinter.PrintOnNextUpdate();
			  progressPrinter.AddBytes( 100 );
			  progressPrinter.EndFile();
			  progressPrinter.Done();
			  progressPrinter.PrintProgress();

			  printStream.flush();
			  string output = bout.ToString();
			  assertEquals( output, "\nFiles: 1/10, data:  0.5%" + "\nFiles: 2/10, data: 20.5%" + "\nFiles: 2/10, data: 20.5%" + "\nFiles: 3/10, data: 30.5%" + "\nFiles: 3/10, data: 30.5%" + "\nDone: 3 files, 305B processed." + Environment.NewLine );
		 }
	}

}