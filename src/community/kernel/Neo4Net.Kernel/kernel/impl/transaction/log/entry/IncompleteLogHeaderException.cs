using System.Text;

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
namespace Neo4Net.Kernel.impl.transaction.log.entry
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;

	/// <summary>
	/// Used to signal an incomplete log header, i.e. if file is smaller than the header.
	/// This exception is still an <seealso cref="IOException"/>, but a specific subclass of it as to make possible
	/// special handling.
	/// </summary>
	public class IncompleteLogHeaderException : IOException
	{
		 public IncompleteLogHeaderException( File file, int readSize ) : base( Template( file, readSize ) )
		 {
		 }

		 public IncompleteLogHeaderException( int readSize ) : base( Template( null, readSize ) )
		 {
		 }

		 private static string Template( File file, int readSize )
		 {
			  StringBuilder builder = new StringBuilder( "Unable to read log version and last committed tx" );
			  if ( file != null )
			  {
					builder.Append( " from '" ).Append( file.AbsolutePath ).Append( '\'' );
			  }
			  builder.Append( ". Was only able to read " ).Append( readSize ).Append( " bytes, but was expecting " ).Append( LOG_HEADER_SIZE );
			  return builder.ToString();
		 }
	}

}