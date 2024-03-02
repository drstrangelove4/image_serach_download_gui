using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace Image_Downloader_GUI
{
    public partial class ImageSearch : Form
    {
        // Tells the program what file type to work with (jpg is the default format from the flickr webpage)
        const string FILETYPE = ".jpg";

        // Giving these global scope to allow the information to be passed around easier. 
        string filepath;
        string last_button_pressed;
        int current_file = 0;


        //-----------------------------------------------------------------------------------------------------


        public ImageSearch()
        {
            InitializeComponent();
            // Allows the backgroup worker to give information to the rest of the program ( I think).
            backgroundWorker1.WorkerReportsProgress = true;
        }


        //-----------------------------------------------------------------------------------------------------


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }


        //-----------------------------------------------------------------------------------------------------


        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                // Reset the progress bar and button press tracker upon search.
                progressBar1.Value = 0;
                last_button_pressed = "";

                // Tell the search to happen in the background
                backgroundWorker1.RunWorkerAsync();
            }
        }


        //-----------------------------------------------------------------------------------------------------


        private void NextButton_Click(object sender, EventArgs e)
        // Loads the next image downloaded by the program.
        {
            try
            {
                string[] files = Directory.GetFiles(filepath);

                // if there is any found finds or the current file we want to view is not the last one in the list.
                if (files.Length > 0 && current_file < files.Length)
                {
                    if (last_button_pressed == "back")
                    {
                        current_file++;
                    }
                    Load_image(files);
                    current_file++;
                    last_button_pressed = "forward";
                }

                // do nothing if these are no files or at end of index
                else { ; }
            }
            // Use try and except block to make the program do nothing if there are no more images in the directory.
            catch (Exception ex) { ; }
        }


        //-----------------------------------------------------------------------------------------------------


        private void previousButton_Click(object sender, EventArgs e)
        // Button that loads the previous image in the folder of downloaded images.
        {
            try
            {
                string[] files = Directory.GetFiles(filepath);

                // If there are any files in the list and we aren't at the last file in the list
                if (files.Length > 0 && current_file > 0)
                {  
                    if (last_button_pressed == "forward")
                    {
                        current_file--;
                    }
               
                    current_file--;
                    last_button_pressed = "back";
                    Load_image(files);

                }
                // do nothing if these are no files or at start of index
                else { ; }
            }
            // Use try and except block to make the program do nothing if there are no more images in the directory.
            catch (Exception ex) { ; }
        }


        //-----------------------------------------------------------------------------------------------------


        void Load_image(string[] filepaths)
        // Loads an image file into the picturebox 
        {
            // All files downloaded from flickr should be a .jpg file. This checks if the file is an image we can use in
            // our picturebox.
            if (filepaths[current_file].ToString().Contains(FILETYPE))
            {
                // Load the file into the picture box and make it fit
                pictureBox1.Load(filepaths[current_file]);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }


        //-----------------------------------------------------------------------------------------------------


        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)            
        // Allows downloading of images. Gives the task to a background worker to prevent the window from freezing.        
        {     
            // User input from text box
            string search_term = textBox1.Text;

            // Base url for all queries 
            const string BASE_URL = "https://flickr.com/search/?text=";

            // Takes user input
            string full_url = BASE_URL + search_term;

            // Get the image links            
            List<string> image_links = Get_Image_Links(full_url);

            // Display an error if there are no images found.
            if (image_links.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Nothing has been found");
            }            
            else
            {
                // Download and save images. Set the current file to 0 - relevant if we start a new search and want
                // To look at new files in the picture box from file 1. 
                Get_Images(image_links, search_term);
                current_file = 0;
            }   


            // ----------------------


            List<string> Get_Image_Links(string url)
            // Get the image links from the html document
            {
                // Create a web scraping object
                var web = new HtmlWeb();
                // Load the webpage
                var document = web.Load(url);
                // select the images from the HTML document
                var images = document.DocumentNode.QuerySelectorAll("img");

                // The list that will store our links 
                List<string> url_links = new List<string>();


                // Iterates over the image links found, adds the missing prefix and saves them to our results list. 
                foreach (var image in images)
                {

                    string image_url = HtmlEntity.DeEntitize(image.Attributes["src"].Value);
                    url_links.Add("http:" + image_url);

                }

                return url_links;
            }


            // ----------------------


            void Get_Images(List<string> image_links, string search_term)
            // Pulls each image from the provided link and saves the image.
            {

                // Create a unique folder name for the images to be saved in
                string current_directory = Directory.GetCurrentDirectory();
                string current_date = DateTime.Now.ToString("dd-MM--HHmm-ss");
                string new_directory_name = $"{current_directory}\\{search_term}-{current_date}";
                filepath = new_directory_name;
                
                // Create the new directory
                Directory.CreateDirectory(new_directory_name);

                // What will be used for our filename. 
                int image_number = 1;               

                // Rip the image from the internet and save the file.
                foreach (string link in image_links)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(link).GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode)
                        {
                            // Get the image as an array of bytes. 
                            byte[] content = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                            // Save the Byte to file 
                            File.WriteAllBytes(new_directory_name + "\\" + image_number.ToString() + FILETYPE, content);    
                        }
                    }
                    // Update the progress bar
                    progressBar1.Value += (100 / image_links.Count);
                    // Increase the image counter to allow for unique filenames.
                    image_number++;
                }
                // Update the progress bar
                progressBar1.Value = 100;
                // Tell the user where the files are saved.
                System.Windows.Forms.MessageBox.Show($"Images downloaded to {new_directory_name}");                
            }
        }
    }
}

