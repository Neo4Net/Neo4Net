/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.com.storecopy
{

	public interface FileMoveAction
	{
		 /// <summary>
		 /// Execute a file move, moving the prepared file to the given {@code toDir}. </summary>
		 /// <param name="toDir"> The target directory of the move operation </param>
		 /// <param name="copyOptions"> </param>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void move(java.io.File toDir, java.nio.file.CopyOption... copyOptions) throws java.io.IOException;
		 void Move( File toDir, params CopyOption[] copyOptions );

		 File File();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static FileMoveAction copyViaFileSystem(java.io.File file, java.io.File basePath)
	//	 {
	//		  Path @base = basePath.toPath();
	//		  return new FileMoveAction()
	//		  {
	//				@@Override public void move(File toDir, CopyOption... copyOptions) throws IOException
	//				{
	//					 Path originalPath = file.toPath();
	//					 Path relativePath = @base.relativize(originalPath);
	//					 Path resolvedPath = toDir.toPath().resolve(relativePath);
	//					 if (!Files.isSymbolicLink(resolvedPath.getParent()))
	//					 {
	//						  Files.createDirectories(resolvedPath.getParent());
	//					 }
	//					 Files.copy(originalPath, resolvedPath, copyOptions);
	//				}
	//
	//				@@Override public File file()
	//				{
	//					 return file;
	//				}
	//		  };
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static FileMoveAction moveViaFileSystem(java.io.File sourceFile, java.io.File sourceDirectory)
	//	 {
	//		  return new FileMoveAction()
	//		  {
	//				@@Override public void move(File toDir, CopyOption... copyOptions) throws IOException
	//				{
	//					 copyViaFileSystem(sourceFile, sourceDirectory).move(toDir, copyOptions);
	//					 if (!sourceFile.delete())
	//					 {
	//						  throw new IOException("Unable to delete source file after copying " + sourceFile);
	//					 }
	//				}
	//
	//				@@Override public File file()
	//				{
	//					 return sourceFile;
	//				}
	//		  };
	//	 }
	}

}